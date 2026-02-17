using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;
using NFSe2026.API.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NFSe2026.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpresaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConsultaCNPJService _consultaCNPJService;
    private readonly IMapper _mapper;
    private readonly ILogger<EmpresaController> _logger;
    private readonly ICriptografiaService _criptografiaService;
    private readonly IAssinaturaXMLService _assinaturaXML;

    public EmpresaController(
        ApplicationDbContext context,
        IConsultaCNPJService consultaCNPJService,
        IMapper mapper,
        ILogger<EmpresaController> logger,
        ICriptografiaService criptografiaService,
        IAssinaturaXMLService assinaturaXML)
    {
        _context = context;
        _consultaCNPJService = consultaCNPJService;
        _mapper = mapper;
        _logger = logger;
        _criptografiaService = criptografiaService;
        _assinaturaXML = assinaturaXML;
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

    [HttpGet("consultar-cnpj/{cnpj}")]
    [AllowAnonymous]
    public async Task<ActionResult<Services.ConsultaCNPJResult>> ConsultarCNPJ(string cnpj)
    {
        try
        {
            var resultado = await _consultaCNPJService.ConsultarCNPJAsync(cnpj);
            
            if (resultado == null)
            {
                return NotFound(new { error = "CNPJ não encontrado" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar CNPJ");
            return StatusCode(500, new { error = "Erro ao consultar CNPJ" });
        }
    }

    [HttpGet("meus-dados")]
    public async Task<ActionResult<EmpresaDTO>> ObterMinhaEmpresa()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Usa Select para evitar carregar campos que podem não existir ainda no banco (ProvedorNFSe)
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new
                {
                    e.Id,
                    e.CNPJ,
                    e.RazaoSocial,
                    e.NomeFantasia,
                    e.InscricaoEstadual,
                    e.InscricaoMunicipal,
                    e.Endereco,
                    e.Numero,
                    e.Complemento,
                    e.Bairro,
                    e.Cidade,
                    e.UF,
                    e.CodigoMunicipio,
                    e.CEP,
                    e.Telefone,
                    e.Email,
                    e.SituacaoCadastral,
                    e.Logotipo,
                    e.DataVencimentoCertificado,
                    e.CertificadoDigital,
                    e.ClientId,
                    e.ClientSecret,
                    e.RegimeEspecialTributacao,
                    e.OptanteSimplesNacional,
                    e.IncentivoFiscal,
                    e.Ativo,
                    e.ProvedorNFSe
                })
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound();
            }

            // Mapeia manualmente para evitar problemas com campos que podem não existir
            var empresaDto = new EmpresaDTO
            {
                Id = empresa.Id,
                CNPJ = empresa.CNPJ,
                RazaoSocial = empresa.RazaoSocial,
                NomeFantasia = empresa.NomeFantasia,
                InscricaoEstadual = empresa.InscricaoEstadual,
                InscricaoMunicipal = empresa.InscricaoMunicipal,
                Endereco = empresa.Endereco,
                Numero = empresa.Numero,
                Complemento = empresa.Complemento,
                Bairro = empresa.Bairro,
                Cidade = empresa.Cidade,
                UF = empresa.UF,
                CodigoMunicipio = empresa.CodigoMunicipio,
                CEP = empresa.CEP,
                Telefone = empresa.Telefone,
                Email = empresa.Email,
                SituacaoCadastral = empresa.SituacaoCadastral,
                Logotipo = empresa.Logotipo,
                DataVencimentoCertificado = empresa.DataVencimentoCertificado,
                TemCertificadoDigital = !string.IsNullOrEmpty(empresa.CertificadoDigital),
                TemClientIdSecret = !string.IsNullOrEmpty(empresa.ClientId) && !string.IsNullOrEmpty(empresa.ClientSecret),
                ProvedorNFSe = empresa.ProvedorNFSe.HasValue ? (int)empresa.ProvedorNFSe.Value : 1, // Default: 1 = Nacional
                RegimeEspecialTributacao = empresa.RegimeEspecialTributacao,
                OptanteSimplesNacional = empresa.OptanteSimplesNacional,
                IncentivoFiscal = empresa.IncentivoFiscal,
                Ativo = empresa.Ativo
            };

            return Ok(empresaDto);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Empresa não identificada no token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados da empresa");
            return StatusCode(500, new { error = "Erro ao buscar dados da empresa" });
        }
    }

    [HttpGet("verificar-certificado")]
    public async Task<ActionResult> VerificarCertificado()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Busca empresa com select explícito para garantir que os campos sejam carregados
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new
                {
                    e.Id,
                    e.CNPJ,
                    e.RazaoSocial,
                    TemCertificadoDigital = !string.IsNullOrWhiteSpace(e.CertificadoDigital),
                    TamanhoCertificado = e.CertificadoDigital != null ? e.CertificadoDigital.Length : 0,
                    TemSenhaCertificado = !string.IsNullOrWhiteSpace(e.SenhaCertificado),
                    TamanhoSenha = e.SenhaCertificado != null ? e.SenhaCertificado.Length : 0,
                    e.DataVencimentoCertificado,
                    e.Ambiente
                })
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound(new { error = "Empresa não encontrada" });
            }

            return Ok(new
            {
                empresaId = empresa.Id,
                cnpj = empresa.CNPJ,
                razaoSocial = empresa.RazaoSocial,
                temCertificadoDigital = empresa.TemCertificadoDigital,
                tamanhoCertificado = empresa.TamanhoCertificado,
                temSenhaCertificado = empresa.TemSenhaCertificado,
                tamanhoSenha = empresa.TamanhoSenha,
                dataVencimentoCertificado = empresa.DataVencimentoCertificado,
                ambiente = empresa.Ambiente.ToString(),
                status = empresa.TemCertificadoDigital && empresa.TemSenhaCertificado 
                    ? "Certificado cadastrado corretamente" 
                    : "Certificado não cadastrado ou incompleto"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar certificado digital");
            return StatusCode(500, new { error = "Erro ao verificar certificado digital" });
        }
    }

    [HttpPut("meus-dados")]
    public async Task<ActionResult<EmpresaDTO>> AtualizarMinhaEmpresa([FromBody] EmpresaUpdateDTO empresaDto)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound();
            }

            // Atualiza apenas os campos que foram informados
            _mapper.Map(empresaDto, empresa);
            
            // Atualiza ProvedorNFSe manualmente se fornecido (para garantir que seja salvo)
            if (empresaDto.ProvedorNFSe.HasValue)
            {
                empresa.ProvedorNFSe = (ProvedorNFSe)empresaDto.ProvedorNFSe.Value;
                _logger.LogInformation("ProvedorNFSe atualizado para: {Provedor}", empresa.ProvedorNFSe);
            }
            else if (empresa.ProvedorNFSe == null)
            {
                // Se não foi fornecido e está null, define o padrão
                empresa.ProvedorNFSe = ProvedorNFSe.Nacional;
            }
            
            empresa.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Empresa {Id} atualizada com sucesso", empresaId);

            // Retorna usando o mesmo método de ObterMinhaEmpresa para garantir consistência
            return await ObterMinhaEmpresa();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Empresa não identificada no token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa");
            return StatusCode(500, new { error = "Erro ao atualizar empresa" });
        }
    }

    [HttpPost("certificado-digital")]
    public async Task<ActionResult> CadastrarCertificadoDigital([FromBody] CertificadoDigitalDTO certificadoDto)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound();
            }

            // Validações básicas
            if (string.IsNullOrWhiteSpace(certificadoDto.CertificadoDigital))
            {
                return BadRequest(new { error = "O certificado digital é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(certificadoDto.SenhaCertificado))
            {
                return BadRequest(new { error = "A senha do certificado é obrigatória" });
            }

            // Log da senha antes de criptografar (apenas informações, nunca o valor real)
            _logger.LogInformation("Criptografando senha do certificado. Tamanho: {Tamanho} caracteres, Contém caracteres especiais: {TemEspeciais}", 
                certificadoDto.SenhaCertificado.Length, 
                certificadoDto.SenhaCertificado.Any(c => !char.IsLetterOrDigit(c)));
            
            // Criptografa a senha antes de armazenar
            string senhaCriptografada;
            try
            {
                senhaCriptografada = _criptografiaService.Criptografar(certificadoDto.SenhaCertificado);
                _logger.LogInformation("Senha criptografada com sucesso. Tamanho Base64: {Tamanho}", senhaCriptografada.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criptografar senha do certificado");
                return BadRequest(new { error = "Erro ao processar a senha do certificado. Verifique se a senha contém apenas caracteres válidos." });
            }

            // Valida tamanho do certificado antes de salvar
            if (certificadoDto.CertificadoDigital.Length > 100000) // ~75KB em Base64 = ~50KB binário (limite razoável)
            {
                _logger.LogWarning("Certificado muito grande: {Tamanho} caracteres. Pode indicar problema no upload.", certificadoDto.CertificadoDigital.Length);
                return BadRequest(new { error = "Certificado digital muito grande. Verifique se o arquivo está correto (máximo ~50KB)." });
            }

            // Valida se a senha está correta com o certificado ANTES de salvar
            // Usa o serviço de assinatura que tem validação robusta com múltiplas tentativas
            DateTime? dataVencimento = null;
            try
            {
                _logger.LogInformation("Validando certificado digital e senha antes de salvar...");
                
                // Usa o serviço de assinatura que tem validação robusta
                var certificado = _assinaturaXML.CarregarCertificado(certificadoDto.CertificadoDigital, certificadoDto.SenhaCertificado);
                
                // Se chegou aqui, a senha está correta e o certificado foi carregado com sucesso
                dataVencimento = certificado.NotAfter;
                
                // Valida se o certificado tem chave privada (necessário para assinar)
                if (!certificado.HasPrivateKey)
                {
                    certificado.Dispose();
                    _logger.LogError("Certificado carregado mas não possui chave privada.");
                    return BadRequest(new { error = "❌ O certificado digital não possui chave privada. Certifique-se de que é um certificado A1 ou A3 com a chave privada incluída." });
                }
                
                // Obtém o tamanho do certificado em bytes para logging
                var certificadoBytes = Convert.FromBase64String(certificadoDto.CertificadoDigital);
                _logger.LogInformation("✅ Certificado e senha validados com sucesso! Tamanho: {TamanhoBytes} bytes, Válido até: {DataVencimento}, Subject: {Subject}", 
                    certificadoBytes.Length, dataVencimento, certificado.Subject);
                
                certificado.Dispose(); // Libera o certificado após validação
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro de validação do certificado: {Mensagem}", ex.Message);
                return BadRequest(new { error = $"❌ {ex.Message}" });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message.ToLower();
                _logger.LogError(ex, "Erro ao validar certificado e senha. Mensagem: {Mensagem}", ex.Message);
                
                // Mensagens mais específicas baseadas no tipo de erro
                if (errorMessage.Contains("senha") || errorMessage.Contains("password") || errorMessage.Contains("incorrect") || errorMessage.Contains("incorreta"))
                {
                    return BadRequest(new { error = "❌ Senha do certificado incorreta. Verifique se a senha está correta, incluindo caracteres especiais como @, #, $, etc. Certifique-se de que não há espaços extras no início ou fim da senha." });
                }
                else if (errorMessage.Contains("truncado") || errorMessage.Contains("truncated"))
                {
                    return BadRequest(new { error = "❌ O certificado digital parece estar truncado ou incompleto. Por favor, recadastre o certificado após atualizar o banco de dados." });
                }
                else if (errorMessage.Contains("formato") || errorMessage.Contains("format") || errorMessage.Contains("base64"))
                {
                    return BadRequest(new { error = "❌ Formato do certificado inválido. Verifique se o arquivo é um certificado .pfx ou .p12 válido e foi carregado completamente." });
                }
                else if (errorMessage.Contains("chave privada") || errorMessage.Contains("private key"))
                {
                    return BadRequest(new { error = "❌ O certificado digital não possui chave privada. Certifique-se de que é um certificado A1 ou A3 com a chave privada incluída." });
                }
                else
                {
                    return BadRequest(new { error = $"❌ Erro ao validar o certificado: {ex.Message}. Verifique se a senha está correta e se o certificado não está corrompido." });
                }
            }

            // Atualiza os dados do certificado
            empresa.CertificadoDigital = certificadoDto.CertificadoDigital; // Base64 do arquivo .pfx ou .p12
            empresa.SenhaCertificado = senhaCriptografada;
            empresa.DataVencimentoCertificado = dataVencimento;
            empresa.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Certificado digital cadastrado com sucesso para empresa {Id}. Data de vencimento: {DataVencimento}", empresaId, dataVencimento);

            // Retorna a data de vencimento para o front-end
            return Ok(new { 
                message = "Certificado digital cadastrado com sucesso",
                dataVencimento = dataVencimento
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Empresa não identificada no token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar certificado digital");
            return StatusCode(500, new { error = "Erro ao cadastrar certificado digital" });
        }
    }

    [HttpDelete("certificado-digital")]
    public async Task<ActionResult> RemoverCertificadoDigital()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound();
            }

            // Remove os dados do certificado
            empresa.CertificadoDigital = null;
            empresa.SenhaCertificado = null;
            empresa.DataVencimentoCertificado = null;
            empresa.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Certificado digital removido com sucesso para empresa {Id}", empresaId);

            return Ok(new { message = "Certificado digital removido com sucesso" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Empresa não identificada no token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover certificado digital");
            return StatusCode(500, new { error = "Erro ao remover certificado digital" });
        }
    }

    [HttpPost("credenciais-api")]
    public async Task<ActionResult> CadastrarCredenciaisAPI([FromBody] CredenciaisAPIDTO credenciaisDto)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound();
            }

            // Validações básicas
            if (string.IsNullOrWhiteSpace(credenciaisDto.ClientId))
            {
                return BadRequest(new { error = "O ClientId (Token) é obrigatório" });
            }

            // ClientSecret é opcional (pode ser usado para NS Tecnologia ou API Nacional)
            // Se não fornecido, pode ser usado o mesmo valor do ClientId ou deixar vazio

            // Criptografa as credenciais antes de armazenar
            string clientIdCriptografado;
            string? clientSecretCriptografado = null;
            try
            {
                clientIdCriptografado = _criptografiaService.Criptografar(credenciaisDto.ClientId);
                
                // Criptografa ClientSecret apenas se fornecido
                if (!string.IsNullOrWhiteSpace(credenciaisDto.ClientSecret))
                {
                    clientSecretCriptografado = _criptografiaService.Criptografar(credenciaisDto.ClientSecret);
                }
                
                _logger.LogInformation("Credenciais da API criptografadas com sucesso para empresa {Id}", empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criptografar credenciais da API");
                return BadRequest(new { error = "Erro ao processar as credenciais da API." });
            }

            // Atualiza as credenciais
            empresa.ClientId = clientIdCriptografado;
            // Atualiza ClientSecret apenas se foi fornecido, caso contrário mantém o valor atual ou null
            if (clientSecretCriptografado != null)
            {
                empresa.ClientSecret = clientSecretCriptografado;
            }
            // Se ClientSecret não foi fornecido e está vazio, pode ser removido ou mantido conforme necessário
            else if (string.IsNullOrWhiteSpace(credenciaisDto.ClientSecret))
            {
                // Para NS Tecnologia, ClientSecret pode ser null ou vazio
                // Mantém o valor atual se não foi fornecido
                _logger.LogInformation("ClientSecret não fornecido, mantendo valor atual ou null para empresa {Id}", empresaId);
            }
            empresa.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Credenciais da API cadastradas com sucesso para empresa {Id}", empresaId);

            return Ok(new { 
                message = "✅ Credenciais da API cadastradas com sucesso"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Empresa não identificada no token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar credenciais da API");
            return StatusCode(500, new { error = "Erro ao cadastrar credenciais da API" });
        }
    }

    [HttpDelete("credenciais-api")]
    public async Task<ActionResult> RemoverCredenciaisAPI()
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound();
            }

            // Remove as credenciais
            empresa.ClientId = null;
            empresa.ClientSecret = null;
            empresa.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Credenciais da API removidas com sucesso para empresa {Id}", empresaId);

            return Ok(new { 
                message = "Credenciais da API removidas com sucesso"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Empresa não identificada no token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover credenciais da API");
            return StatusCode(500, new { error = "Erro ao remover credenciais da API" });
        }
    }
}

