using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;
using NFSe2026.API.Services;
using System.Security.Claims;

namespace NFSe2026.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TomadorController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TomadorController> _logger;
    private readonly IConsultaCNPJService _consultaCNPJService;
    private readonly TomadorEditLogger _tomadorEditLogger;

    public TomadorController(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<TomadorController> logger,
        IConsultaCNPJService consultaCNPJService,
        TomadorEditLogger tomadorEditLogger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _consultaCNPJService = consultaCNPJService;
        _tomadorEditLogger = tomadorEditLogger;
    }

    private int ObterEmpresaId()
    {
        var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
        {
            throw new UnauthorizedAccessException("Empresa não identificada no token");
        }
        return empresaId;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDTO<TomadorDTO>>> GetTomadores([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Validação de parâmetros
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Tomadores
                .Where(t => t.EmpresaId == empresaId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var tomadores = await query
                .OrderByDescending(t => t.DataCriacao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var tomadoresDTO = _mapper.Map<IEnumerable<TomadorDTO>>(tomadores);

            var resultado = new PagedResultDTO<TomadorDTO>
            {
                Items = tomadoresDTO,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            return Ok(resultado);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Empresa não identificada no token" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TomadorDTO>> GetTomador(int id)
    {
        var empresaId = ObterEmpresaId();
        var tomador = await _context.Tomadores
            .FirstOrDefaultAsync(t => t.Id == id && t.EmpresaId == empresaId);

        if (tomador == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<TomadorDTO>(tomador));
    }

    [HttpPost]
    public async Task<ActionResult<TomadorDTO>> CreateTomador(TomadorCreateDTO tomadorDto)
    {
        var empresaId = ObterEmpresaId();
        var tomador = _mapper.Map<Models.Tomador>(tomadorDto);
        tomador.EmpresaId = empresaId;
        tomador.DataCriacao = DateTime.UtcNow;

        _context.Tomadores.Add(tomador);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tomador {Id} criado com sucesso para empresa {EmpresaId}", tomador.Id, empresaId);

        return CreatedAtAction(nameof(GetTomador), new { id = tomador.Id },
            _mapper.Map<TomadorDTO>(tomador));
    }

    [HttpPost("por-cnpj")]
    public async Task<ActionResult<TomadorDTO>> CreateTomadorPorCNPJ([FromBody] TomadorPorCNPJDTO dto)
    {
        try
        {
            // Remove formatação do CNPJ
            var cnpj = dto.CNPJ.Replace(".", "").Replace("/", "").Replace("-", "").Trim();

            if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length != 14)
            {
                return BadRequest(new { error = "CNPJ inválido. Deve conter 14 dígitos." });
            }

            var empresaId = ObterEmpresaId();

            // Verifica se já existe tomador com este CNPJ para esta empresa
            var tomadorExistente = await _context.Tomadores
                .FirstOrDefaultAsync(t => t.CPFCNPJ == cnpj && t.EmpresaId == empresaId);

            if (tomadorExistente != null)
            {
                return Conflict(new { error = "Já existe um tomador cadastrado com este CNPJ." });
            }

            // Consulta dados do CNPJ na Brasil API
            var dadosCNPJ = await _consultaCNPJService.ConsultarCNPJAsync(cnpj);

            if (dadosCNPJ == null)
            {
                return BadRequest(new { error = "Não foi possível consultar os dados do CNPJ. Verifique se o CNPJ está correto." });
            }

            // Valida se tem dados mínimos necessários
            if (string.IsNullOrWhiteSpace(dadosCNPJ.RazaoSocial))
            {
                return BadRequest(new { error = "Não foi possível obter a razão social do CNPJ." });
            }

            if (dadosCNPJ.Endereco == null)
            {
                return BadRequest(new { error = "Não foi possível obter o endereço do CNPJ." });
            }

            // Cria o tomador com os dados da API
            var tomador = new Models.Tomador
            {
                TipoPessoa = Models.TipoPessoa.Juridica, // CNPJ sempre é pessoa jurídica
                CPFCNPJ = cnpj,
                RazaoSocialNome = dadosCNPJ.RazaoSocial,
                InscricaoEstadual = dadosCNPJ.InscricaoEstadual,
                InscricaoMunicipal = dadosCNPJ.InscricaoMunicipal,
                Endereco = dadosCNPJ.Endereco.Logradouro,
                Numero = dadosCNPJ.Endereco.Numero,
                Complemento = dadosCNPJ.Endereco.Complemento,
                Bairro = dadosCNPJ.Endereco.Bairro,
                Cidade = dadosCNPJ.Endereco.Cidade,
                UF = dadosCNPJ.Endereco.UF,
                CEP = dadosCNPJ.Endereco.CEP,
                Email = dadosCNPJ.Email,
                Telefone = dadosCNPJ.Telefone,
                EmpresaId = empresaId,
                DataCriacao = DateTime.UtcNow
            };

            // Valida campos obrigatórios
            if (string.IsNullOrWhiteSpace(tomador.Numero))
            {
                tomador.Numero = "S/N"; // S/N se não tiver número
            }

            _context.Tomadores.Add(tomador);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tomador {Id} criado com sucesso a partir do CNPJ {CNPJ}", tomador.Id, cnpj);

            return CreatedAtAction(nameof(GetTomador), new { id = tomador.Id },
                _mapper.Map<TomadorDTO>(tomador));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar tomador por CNPJ {CNPJ}", dto.CNPJ);
            return StatusCode(500, new { error = "Erro interno ao processar a solicitação." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTomador(int id, TomadorUpdateDTO tomadorDto)
    {
        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
        _logger.LogInformation("UPDATE TOMADOR CHAMADO - ID: {Id}", id);
        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
        
        var empresaId = ObterEmpresaId();
        
        // PASSO 1 - Buscar tomador
        _tomadorEditLogger.LogEdicao(_tomadorEditLogger.FormatLogCompleto(
            id,
            "PASSO 1 - BUSCAR TOMADOR",
            "Buscando tomador no banco de dados",
            new Dictionary<string, object>
            {
                { "TomadorId", id },
                { "EmpresaId", empresaId }
            }));
        
        var tomador = await _context.Tomadores
            .FirstOrDefaultAsync(t => t.Id == id && t.EmpresaId == empresaId);
        
        if (tomador == null)
        {
            _tomadorEditLogger.LogEdicao(_tomadorEditLogger.FormatLogCompleto(
                id,
                "PASSO 1.1 - RESULTADO",
                "Tomador não encontrado",
                new Dictionary<string, object>
                {
                    { "TomadorId", id },
                    { "EmpresaId", empresaId },
                    { "Resultado", "NOT_FOUND" }
                }));
            return NotFound();
        }

        // PASSO 2 - Dados recebidos
        _logger.LogInformation("UpdateTomador - Email recebido: '{Email}', Telefone recebido: '{Telefone}'", 
            tomadorDto.Email ?? "null", tomadorDto.Telefone ?? "null");
        
        _tomadorEditLogger.LogEdicao(_tomadorEditLogger.FormatLogCompleto(
            id,
            "PASSO 2 - DADOS RECEBIDOS",
            "Dados recebidos do cliente",
            new Dictionary<string, object>
            {
                { "TipoPessoa", tomadorDto.TipoPessoa },
                { "RazaoSocialNome", tomadorDto.RazaoSocialNome ?? "null" },
                { "InscricaoEstadual", tomadorDto.InscricaoEstadual ?? "null" },
                { "InscricaoMunicipal", tomadorDto.InscricaoMunicipal ?? "null" },
                { "Endereco", tomadorDto.Endereco ?? "null" },
                { "Numero", tomadorDto.Numero ?? "null" },
                { "Complemento", tomadorDto.Complemento ?? "null" },
                { "Bairro", tomadorDto.Bairro ?? "null" },
                { "Cidade", tomadorDto.Cidade ?? "null" },
                { "UF", tomadorDto.UF ?? "null" },
                { "CEP", tomadorDto.CEP ?? "null" },
                { "Email", tomadorDto.Email ?? "null" },
                { "Telefone", tomadorDto.Telefone ?? "null" }
            }));

        // PASSO 3 - Dados anteriores (antes da atualização)
        _tomadorEditLogger.LogEdicao(_tomadorEditLogger.FormatLogCompleto(
            id,
            "PASSO 3 - DADOS ANTERIORES",
            "Valores atuais no banco de dados (antes da atualização)",
            new Dictionary<string, object>
            {
                { "TipoPessoa", tomador.TipoPessoa },
                { "RazaoSocialNome", tomador.RazaoSocialNome ?? "null" },
                { "InscricaoEstadual", tomador.InscricaoEstadual ?? "null" },
                { "InscricaoMunicipal", tomador.InscricaoMunicipal ?? "null" },
                { "Endereco", tomador.Endereco ?? "null" },
                { "Numero", tomador.Numero ?? "null" },
                { "Complemento", tomador.Complemento ?? "null" },
                { "Bairro", tomador.Bairro ?? "null" },
                { "Cidade", tomador.Cidade ?? "null" },
                { "UF", tomador.UF ?? "null" },
                { "CEP", tomador.CEP ?? "null" },
                { "Email", tomador.Email ?? "null" },
                { "Telefone", tomador.Telefone ?? "null" }
            }));

        // PASSO 4 - Atualizar campos
        _tomadorEditLogger.LogEdicao(_tomadorEditLogger.FormatLogCompleto(
            id,
            "PASSO 4 - ATUALIZAR CAMPOS",
            "Aplicando novos valores aos campos",
            new Dictionary<string, object>
            {
                { "Operacao", "Atualização de campos" }
            }));

        // Mapeia os campos manualmente para garantir que campos nullable sejam atualizados corretamente
        tomador.TipoPessoa = (TipoPessoa)tomadorDto.TipoPessoa;
        tomador.RazaoSocialNome = tomadorDto.RazaoSocialNome;
        tomador.InscricaoEstadual = tomadorDto.InscricaoEstadual;
        tomador.InscricaoMunicipal = tomadorDto.InscricaoMunicipal;
        tomador.Endereco = tomadorDto.Endereco;
        tomador.Numero = tomadorDto.Numero;
        tomador.Complemento = tomadorDto.Complemento;
        tomador.Bairro = tomadorDto.Bairro;
        tomador.Cidade = tomadorDto.Cidade;
        tomador.UF = tomadorDto.UF;
        tomador.CEP = tomadorDto.CEP;
        // Normaliza string vazia para null para campos nullable
        tomador.Email = string.IsNullOrWhiteSpace(tomadorDto.Email) ? null : tomadorDto.Email.Trim();
        tomador.Telefone = string.IsNullOrWhiteSpace(tomadorDto.Telefone) ? null : tomadorDto.Telefone.Trim();
        tomador.DataAtualizacao = DateTime.UtcNow;

        _logger.LogInformation("UpdateTomador - Email após normalização: '{Email}', Telefone após normalização: '{Telefone}'", 
            tomador.Email ?? "null", tomador.Telefone ?? "null");

        // PASSO 5 - Salvar no banco
        _tomadorEditLogger.LogEdicao(_tomadorEditLogger.FormatLogCompleto(
            id,
            "PASSO 5 - SALVAR NO BANCO",
            "Persistindo alterações no banco de dados",
            new Dictionary<string, object>
            {
                { "Operacao", "SaveChangesAsync" }
            }));

        await _context.SaveChangesAsync();
        
        // PASSO 6 - Dados finais (após atualização)
        _logger.LogInformation("Tomador {Id} atualizado com sucesso. Email salvo: '{Email}'", tomador.Id, tomador.Email ?? "null");
        
        _tomadorEditLogger.LogEdicao(_tomadorEditLogger.FormatLogCompleto(
            id,
            "PASSO 6 - DADOS FINAIS",
            "Valores salvos no banco de dados (após atualização)",
            new Dictionary<string, object>
            {
                { "TipoPessoa", tomador.TipoPessoa },
                { "RazaoSocialNome", tomador.RazaoSocialNome ?? "null" },
                { "InscricaoEstadual", tomador.InscricaoEstadual ?? "null" },
                { "InscricaoMunicipal", tomador.InscricaoMunicipal ?? "null" },
                { "Endereco", tomador.Endereco ?? "null" },
                { "Numero", tomador.Numero ?? "null" },
                { "Complemento", tomador.Complemento ?? "null" },
                { "Bairro", tomador.Bairro ?? "null" },
                { "Cidade", tomador.Cidade ?? "null" },
                { "UF", tomador.UF ?? "null" },
                { "CEP", tomador.CEP ?? "null" },
                { "Email", tomador.Email ?? "null" },
                { "Telefone", tomador.Telefone ?? "null" },
                { "DataAtualizacao", tomador.DataAtualizacao.HasValue ? tomador.DataAtualizacao.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null" },
                { "Resultado", "SUCESSO" }
            }));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTomador(int id)
    {
        var empresaId = ObterEmpresaId();
        var tomador = await _context.Tomadores
            .FirstOrDefaultAsync(t => t.Id == id && t.EmpresaId == empresaId);
        
        if (tomador == null)
        {
            return NotFound();
        }

        _context.Tomadores.Remove(tomador);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

