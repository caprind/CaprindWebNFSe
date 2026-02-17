using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;
using NFSe2026.API.Services;
using System.IO;
using System.Security.Claims;

namespace NFSe2026.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotaFiscalController : ControllerBase
{
    private readonly INotaFiscalService _notaFiscalService;
    private readonly IProvedorNFSeFactory _provedorFactory;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotaFiscalController> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _pastaPDF;
    private readonly IEmailService _emailService;

    public NotaFiscalController(
        INotaFiscalService notaFiscalService,
        IProvedorNFSeFactory provedorFactory,
        ApplicationDbContext context,
        ILogger<NotaFiscalController> logger,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _notaFiscalService = notaFiscalService;
        _provedorFactory = provedorFactory;
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _pastaPDF = _configuration["Documentos:PastaPDF"] ?? "Documentos/Nota Fiscal/PDF";
        _emailService = emailService;
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
    public async Task<ActionResult<IEnumerable<NotaFiscalDTO>>> GetNotasFiscais([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var empresaId = ObterEmpresaId();
        
        // Validação de parâmetros
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var resultado = await _notaFiscalService.ListarNotasFiscaisPaginadasAsync(empresaId, page, pageSize);
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotaFiscalDTO>> GetNotaFiscal(int id)
    {
        var empresaId = ObterEmpresaId();
        var nota = await _notaFiscalService.ObterNotaFiscalPorIdAsync(id, empresaId);

        if (nota == null)
        {
            return NotFound();
        }

        return Ok(nota);
    }

    [HttpPost]
    public async Task<ActionResult<NotaFiscalDTO>> CreateNotaFiscal(NotaFiscalCreateDTO notaFiscalDto)
    {
        try
        {
            // Log para debug
            _logger.LogInformation("Recebendo criação de nota fiscal. TomadorId: {TomadorId}, ItensServico: {Count}", 
                notaFiscalDto.TomadorId, 
                notaFiscalDto.ItensServico?.Count ?? 0);

            if (notaFiscalDto.ItensServico == null || !notaFiscalDto.ItensServico.Any())
            {
                _logger.LogWarning("Nota fiscal criada sem itens de serviço");
                return BadRequest(new { error = "A nota fiscal deve ter pelo menos um item de serviço. Verifique se os itens foram enviados corretamente." });
            }

            var empresaId = ObterEmpresaId();
            var nota = await _notaFiscalService.CriarNotaFiscalAsync(notaFiscalDto, empresaId);
            return CreatedAtAction(nameof(GetNotaFiscal), new { id = nota.Id }, nota);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validação falhou ao criar nota fiscal");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar nota fiscal");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<NotaFiscalDTO>> UpdateNotaFiscal(int id, [FromBody] NotaFiscalUpdateDTO notaFiscalDto)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var nota = await _notaFiscalService.AtualizarNotaFiscalAsync(id, notaFiscalDto, empresaId);
            return Ok(nota);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar nota fiscal {Id}", id);
            return StatusCode(500, new { error = $"Erro ao atualizar nota fiscal: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotaFiscal(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var sucesso = await _notaFiscalService.ExcluirNotaFiscalAsync(id, empresaId);

            if (!sucesso)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao excluir nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir nota fiscal {Id}", id);
            return StatusCode(500, new { error = "Erro ao excluir nota fiscal" });
        }
    }

    [HttpPost("{id}/emitir")]
    public async Task<ActionResult<NotaFiscalDTO>> EmitirNotaFiscal(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            _logger.LogInformation("Iniciando emissão da nota fiscal {Id} para empresa {EmpresaId}", id, empresaId);
            var nota = await _notaFiscalService.EmitirNotaFiscalAsync(id, empresaId);
            _logger.LogInformation("Nota fiscal {Id} emitida com sucesso", id);
            return Ok(nota);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao emitir nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Erro de autorização ao emitir nota fiscal {Id}", id);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal {Id}", id);
            
            // Retorna a mensagem de erro completa para ajudar no diagnóstico
            var mensagemErro = ex.Message;
            
            // Se a mensagem contém informações sobre certificado ou token, mantém a mensagem original
            if (mensagemErro.Contains("certificado") || mensagemErro.Contains("token") || mensagemErro.Contains("404") || mensagemErro.Contains("401"))
            {
                return StatusCode(500, new { error = mensagemErro });
            }
            
            // Caso contrário, retorna uma mensagem genérica com detalhes no log
            return StatusCode(500, new { error = $"Erro ao emitir nota fiscal. Verifique os logs para mais detalhes. Erro: {mensagemErro}" });
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPDF(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var nota = await _notaFiscalService.ObterNotaFiscalPorIdAsync(id, empresaId);

            if (nota == null)
            {
                return NotFound(new { error = "Nota fiscal não encontrada" });
            }

            // Busca a nota completa para obter o PDFUrl, nsNRec, Numero e CodigoVerificacao
            var notaCompleta = await _context.NotasFiscais
                .Include(n => n.Tomador)
                .Where(n => n.Id == id && n.EmpresaId == empresaId)
                .FirstOrDefaultAsync();

            if (notaCompleta == null)
            {
                return NotFound(new { error = "Nota fiscal não encontrada" });
            }

            // Primeiro tenta usar o arquivo salvo, se existir (prioridade máxima)
            if (!string.IsNullOrWhiteSpace(notaCompleta.PDFUrl) && System.IO.File.Exists(notaCompleta.PDFUrl))
            {
                _logger.LogInformation("Lendo PDF salvo do arquivo: {Caminho}", notaCompleta.PDFUrl);
                var pdfBytesSalvos = await System.IO.File.ReadAllBytesAsync(notaCompleta.PDFUrl);
                var fileName = System.IO.Path.GetFileName(notaCompleta.PDFUrl);
                Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");
                return File(pdfBytesSalvos, "application/pdf");
            }

            // Se não tem PDF salvo mas a nota está autorizada, tenta baixar novamente
            if (notaCompleta.Situacao == SituacaoNotaFiscal.Autorizada)
            {
                _logger.LogInformation("PDF não encontrado no sistema de arquivos, mas nota está autorizada. Tentando baixar novamente...");
            }
            else if (string.IsNullOrWhiteSpace(notaCompleta.Numero) || string.IsNullOrWhiteSpace(notaCompleta.CodigoVerificacao))
            {
                return BadRequest(new { error = "Nota fiscal não foi emitida ainda ou PDF não está disponível. Apenas notas autorizadas podem ter PDF baixado." });
            }

            // Se não encontrou arquivo salvo, tenta fazer download da API (fallback)
            _logger.LogInformation("PDF não encontrado no sistema de arquivos. Tentando fazer download da API...");
            
            // Busca o provedor configurado para a empresa
            var provedor = await _provedorFactory.ObterProvedorAsync(empresaId);
            
            // Busca CNPJ da empresa
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new { e.CNPJ })
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound(new { error = "Empresa não encontrada" });
            }

            // PASSO 2 - Consulta o status para obter as chaves chDPS e chNFSe
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
            _logger.LogInformation("🔍 PASSO 2 - CONSULTA STATUS DE PROCESSAMENTO");
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
            _logger.LogInformation("Consultando status da nota fiscal {Id} - Numero: {Numero}, CNPJ: {CNPJ}", 
                id, notaCompleta.Numero ?? "null", empresa.CNPJ);
            
            var nsNRec = notaCompleta.NsNRec ?? notaCompleta.Numero ?? id.ToString();
            _logger.LogInformation("Usando nsNRec: {NsNRec} para consultar status", nsNRec);
            
            var statusResult = await provedor.ConsultarStatusAsync(
                nsNRec,
                empresa.CNPJ,
                empresaId,
                id, // Passa o ID da nota fiscal para os logs
                notaCompleta.Numero); // Passa o número da nota para os logs
            
            if (statusResult == null || string.IsNullOrWhiteSpace(statusResult.ChDPS) || string.IsNullOrWhiteSpace(statusResult.ChNFSe))
            {
                _logger.LogWarning("❌ PASSO 2 FALHOU - Não foi possível obter as chaves chDPS e chNFSe da consulta de status.");
                _logger.LogWarning("   StatusResult é null: {IsNull}", statusResult == null);
                if (statusResult != null)
                {
                    _logger.LogWarning("   ChDPS: {ChDPS}, ChNFSe: {ChNFSe}", 
                        string.IsNullOrWhiteSpace(statusResult.ChDPS) ? "vazio" : "preenchido",
                        string.IsNullOrWhiteSpace(statusResult.ChNFSe) ? "vazio" : "preenchido");
                }
                return NotFound(new { error = "Não foi possível obter as chaves de acesso da nota fiscal. A nota pode não ter sido processada ainda ou o número do protocolo (nsNRec) não foi encontrado." });
            }

            _logger.LogInformation("✅ PASSO 2 CONCLUÍDO - Chaves obtidas com sucesso:");
            _logger.LogInformation("   - chDPS: {ChDPS}", statusResult.ChDPS);
            _logger.LogInformation("   - chNFSe: {ChNFSe}", statusResult.ChNFSe);
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");

            // PASSO 3 - Download do PDF usando as chaves obtidas
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
            _logger.LogInformation("📥 PASSO 3 - DOWNLOAD DO PDF");
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
            _logger.LogInformation("Fazendo download do PDF da nota fiscal {Id} usando chDPS e chNFSe", id);
            
            var pdfBytes = await provedor.DownloadPDFAsync(statusResult.ChDPS, statusResult.ChNFSe, empresa.CNPJ, empresaId);
            
            if (pdfBytes != null && pdfBytes.Length > 0)
            {
                _logger.LogInformation("✅ PASSO 3 CONCLUÍDO - PDF baixado com sucesso. Tamanho: {Tamanho} bytes", pdfBytes.Length);
                
                // Salva o PDF novamente na pasta (mesma lógica do NotaFiscalService)
                try
                {
                    var pastaBase = System.IO.Path.IsPathRooted(_pastaPDF) ? _pastaPDF : System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), _pastaPDF);
                    
                    // Remove formatação do CNPJ para usar como nome da pasta
                    var cnpjEmpresa = empresa.CNPJ?.Replace(".", "").Replace("/", "").Replace("-", "").Trim() ?? "SEM_CNPJ";
                    var pastaCompleta = System.IO.Path.Combine(pastaBase, cnpjEmpresa);
                    
                    // Cria a pasta se não existir
                    if (!System.IO.Directory.Exists(pastaCompleta))
                    {
                        System.IO.Directory.CreateDirectory(pastaCompleta);
                        _logger.LogInformation("Pasta de documentos PDF criada: {PastaCompleta}", pastaCompleta);
                    }
                    
                    // Formata o número da nota com 6 dígitos (ex: 000006)
                    var numeroFormatado = "000000";
                    if (!string.IsNullOrWhiteSpace(notaCompleta.Numero) && int.TryParse(notaCompleta.Numero, out var numero))
                    {
                        numeroFormatado = numero.ToString("D6");
                    }
                    
                    // Obtém o nome do tomador
                    var nomeTomador = "Sem Tomador";
                    if (notaCompleta.Tomador != null && !string.IsNullOrWhiteSpace(notaCompleta.Tomador.RazaoSocialNome))
                    {
                        nomeTomador = notaCompleta.Tomador.RazaoSocialNome;
                    }
                    
                    // Remove caracteres inválidos do nome do arquivo
                    var caracteresInvalidos = System.IO.Path.GetInvalidFileNameChars();
                    foreach (var c in caracteresInvalidos)
                    {
                        nomeTomador = nomeTomador.Replace(c, '_');
                    }
                    
                    // Nome do arquivo: {NumeroFormatado} - {NomeTomador}.pdf
                    var nomeArquivo = $"{numeroFormatado} - {nomeTomador}.pdf";
                    
                    var caminhoCompleto = System.IO.Path.Combine(pastaCompleta, nomeArquivo);
                    
                    await System.IO.File.WriteAllBytesAsync(caminhoCompleto, pdfBytes);
                    
                    // Atualiza o caminho do PDF na nota fiscal
                    notaCompleta.PDFUrl = caminhoCompleto;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("✅ PDF salvo novamente com sucesso: {Caminho}", caminhoCompleto);
                    
                    // Retorna o PDF usando o nome do arquivo gerado
                    var fileNameRetorno = System.IO.Path.GetFileName(caminhoCompleto);
                    Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileNameRetorno}\"");
                    return File(pdfBytes, "application/pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao salvar PDF da nota fiscal {Id} (não crítico, PDF será retornado mesmo assim)", id);
                    // Retorna o PDF mesmo se não conseguir salvar
                    var numeroFormatadoFallback = !string.IsNullOrWhiteSpace(notaCompleta.Numero) && int.TryParse(notaCompleta.Numero, out var num) ? num.ToString("D6") : (notaCompleta.Numero ?? id.ToString());
                    var fileNameFallback = $"NFSe_{numeroFormatadoFallback}_{DateTime.Now:yyyyMMdd}.pdf";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileNameFallback}\"");
                    return File(pdfBytes, "application/pdf");
                }
            }
            else
            {
                _logger.LogWarning("❌ PASSO 3 FALHOU - PDF não foi retornado ou está vazio");
                _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                return NotFound(new { error = "PDF não disponível para esta nota fiscal" });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Erro de autorização ao baixar PDF da nota fiscal {Id}", id);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar PDF da nota fiscal {Id}", id);
            return StatusCode(500, new { error = "Erro ao baixar PDF da nota fiscal" });
        }
    }

    [HttpPost("{id}/enviar-email")]
    public async Task<IActionResult> EnviarPDFPorEmail(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            
            // Busca a nota fiscal com tomador
            var notaCompleta = await _context.NotasFiscais
                .Include(n => n.Tomador)
                .Where(n => n.Id == id && n.EmpresaId == empresaId)
                .FirstOrDefaultAsync();

            if (notaCompleta == null)
            {
                return NotFound(new { error = "Nota fiscal não encontrada" });
            }

            // Verifica se a nota está autorizada
            if (notaCompleta.Situacao != SituacaoNotaFiscal.Autorizada)
            {
                return BadRequest(new { error = "Apenas notas autorizadas podem ter PDF enviado por email" });
            }

            // Verifica se o tomador tem email cadastrado
            if (notaCompleta.Tomador == null || string.IsNullOrWhiteSpace(notaCompleta.Tomador.Email))
            {
                return BadRequest(new { error = "O tomador não possui email cadastrado" });
            }

            // Obtém o PDF (do arquivo ou baixa novamente)
            byte[]? pdfBytes = null;
            string? nomeArquivo = null;

            // Tenta ler do arquivo salvo
            if (!string.IsNullOrWhiteSpace(notaCompleta.PDFUrl) && System.IO.File.Exists(notaCompleta.PDFUrl))
            {
                pdfBytes = await System.IO.File.ReadAllBytesAsync(notaCompleta.PDFUrl);
                nomeArquivo = System.IO.Path.GetFileName(notaCompleta.PDFUrl);
                _logger.LogInformation("PDF carregado do arquivo salvo: {Caminho}", notaCompleta.PDFUrl);
            }
            else
            {
                // Se não existe arquivo, baixa novamente
                _logger.LogInformation("PDF não encontrado no sistema de arquivos. Baixando novamente...");
                
                var provedor = await _provedorFactory.ObterProvedorAsync(empresaId);
                
                var empresa = await _context.Empresas
                    .Where(e => e.Id == empresaId)
                    .Select(e => new { e.CNPJ })
                    .FirstOrDefaultAsync();

                if (empresa == null)
                {
                    return NotFound(new { error = "Empresa não encontrada" });
                }

                var nsNRec = notaCompleta.NsNRec ?? notaCompleta.Numero ?? id.ToString();
                var statusResult = await provedor.ConsultarStatusAsync(nsNRec, empresa.CNPJ, empresaId, id, notaCompleta.Numero);
                
                if (statusResult == null || string.IsNullOrWhiteSpace(statusResult.ChDPS) || string.IsNullOrWhiteSpace(statusResult.ChNFSe))
                {
                    return NotFound(new { error = "Não foi possível obter as chaves de acesso da nota fiscal" });
                }

                pdfBytes = await provedor.DownloadPDFAsync(statusResult.ChDPS, statusResult.ChNFSe, empresa.CNPJ, empresaId);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return NotFound(new { error = "PDF não disponível para esta nota fiscal" });
                }

                // Gera nome do arquivo
                var numeroFormatado = !string.IsNullOrWhiteSpace(notaCompleta.Numero) && int.TryParse(notaCompleta.Numero, out var num) 
                    ? num.ToString("D6") 
                    : (notaCompleta.Numero ?? "000000");
                var nomeTomador = notaCompleta.Tomador?.RazaoSocialNome ?? "Sem Tomador";
                var caracteresInvalidos = System.IO.Path.GetInvalidFileNameChars();
                foreach (var c in caracteresInvalidos)
                {
                    nomeTomador = nomeTomador.Replace(c, '_');
                }
                nomeArquivo = $"{numeroFormatado} - {nomeTomador}.pdf";
            }

            // Envia o email
            var numeroNota = notaCompleta.Numero ?? id.ToString();
            var nomeTomadorEmail = notaCompleta.Tomador.RazaoSocialNome;
            
            await _emailService.EnviarPDFPorEmailAsync(
                notaCompleta.Tomador.Email,
                nomeTomadorEmail,
                numeroNota,
                pdfBytes,
                nomeArquivo ?? $"NFSe_{numeroNota}.pdf"
            );

            return Ok(new { message = "PDF enviado por email com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar PDF por email da nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar PDF por email da nota fiscal {Id}", id);
            return StatusCode(500, new { error = $"Erro ao enviar PDF por email: {ex.Message}" });
        }
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> CancelarNotaFiscal(int id, [FromBody] CancelarNotaFiscalDTO cancelarDto)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var sucesso = await _notaFiscalService.CancelarNotaFiscalAsync(id, cancelarDto.Motivo, empresaId);

            if (!sucesso)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao cancelar nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal {Id}", id);
            // Retorna a mensagem de erro real ao invés de uma mensagem genérica
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { error = $"Erro ao cancelar nota fiscal: {errorMessage}" });
        }
    }

    [HttpGet("{id}/consultar")]
    public async Task<ActionResult<NotaFiscalDTO>> ConsultarSituacao(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var nota = await _notaFiscalService.ConsultarSituacaoAsync(id, empresaId);

            if (nota == null)
            {
                return NotFound();
            }

            return Ok(nota);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar situação da nota fiscal");
            return StatusCode(500, new { error = "Erro ao consultar situação da nota fiscal" });
        }
    }

    [HttpPost("{id}/consultar-status")]
    public async Task<ActionResult<NotaFiscalDTO>> ConsultarStatus(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var nota = await _notaFiscalService.ConsultarStatusAsync(id, empresaId);

            if (nota == null)
            {
                return NotFound(new { error = "Nota fiscal não encontrada" });
            }

            return Ok(nota);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validação falhou ao consultar status da nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status da nota fiscal {Id}", id);
            return StatusCode(500, new { error = "Erro ao consultar status da nota fiscal" });
        }
    }

    [HttpGet("{id}/xml")]
    public async Task<ActionResult> GetXML(int id)
    {
        var empresaId = ObterEmpresaId();
        var xml = await _notaFiscalService.ObterXMLAsync(id, empresaId);

        if (xml == null)
        {
            return NotFound();
        }

        return Content(xml, "application/xml");
    }

    [HttpGet("{id}/motivo-rejeicao")]
    public async Task<ActionResult<object>> GetMotivoRejeicao(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var motivo = await _notaFiscalService.ObterMotivoRejeicaoAsync(id, empresaId);
            return Ok(new { motivoRejeicao = motivo });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao obter motivo de rejeição da nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter motivo de rejeição da nota fiscal {Id}", id);
            return StatusCode(500, new { error = "Erro ao obter motivo de rejeição" });
        }
    }

    [HttpPost("{id}/reverter-para-rascunho")]
    public async Task<ActionResult<NotaFiscalDTO>> ReverterParaRascunho(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var nota = await _notaFiscalService.ReverterParaRascunhoAsync(id, empresaId);
            return Ok(nota);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao reverter nota fiscal {Id} para Rascunho", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reverter nota fiscal {Id} para Rascunho", id);
            return StatusCode(500, new { error = "Erro ao reverter nota fiscal para Rascunho" });
        }
    }

    [HttpPost("{id}/copiar")]
    public async Task<ActionResult<NotaFiscalDTO>> CopiarNotaFiscal(int id)
    {
        try
        {
            var empresaId = ObterEmpresaId();
            var nota = await _notaFiscalService.CopiarNotaFiscalAsync(id, empresaId);
            return Ok(nota);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao copiar nota fiscal {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao copiar nota fiscal {Id}", id);
            return StatusCode(500, new { error = "Erro ao copiar nota fiscal" });
        }
    }
}

