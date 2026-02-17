using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NFSe2026.Web.Models;
using NFSe2026.Web.Services;

namespace NFSe2026.Web.Controllers;

public class NotaFiscalController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<NotaFiscalController> _logger;
    private readonly IConfiguration _configuration;

    public NotaFiscalController(ApiService apiService, ILogger<NotaFiscalController> logger, IConfiguration configuration)
    {
        _apiService = apiService;
        _logger = logger;
        _configuration = configuration;
    }

    [Route("NotaFiscal/{id}/pdf")]
    [HttpGet]
    public async Task<IActionResult> DownloadPDF(int id)
    {
        try
        {
            // Verifica se há token na sessão
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Faz a requisição para a API usando HttpClient com o token
            using var httpClient = new HttpClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5215";
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.GetAsync($"{apiBaseUrl}/api/notafiscal/{id}/pdf");
            
            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"NFSe_{id}_{DateTime.Now:yyyyMMdd}.pdf";
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
                return File(pdfBytes, "application/pdf");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("PDF não encontrado para nota fiscal {Id}. Resposta: {Response}", id, errorContent);
                TempData["ErrorMessage"] = "PDF não encontrado para esta nota fiscal. A nota pode não ter sido autorizada ainda.";
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Erro de validação ao baixar PDF da nota fiscal {Id}. Resposta: {Response}", id, errorContent);
                
                // Tenta extrair a mensagem de erro do JSON
                try
                {
                    var errorJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                    if (errorJson != null && errorJson.ContainsKey("error"))
                    {
                        var errorMessage = errorJson["error"]?.ToString();
                        TempData["ErrorMessage"] = errorMessage ?? "Não foi possível baixar o PDF. Verifique se a nota foi autorizada.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Não foi possível baixar o PDF. A nota fiscal precisa estar autorizada para ter PDF disponível.";
                    }
                }
                catch
                {
                    TempData["ErrorMessage"] = "Não foi possível baixar o PDF. A nota fiscal precisa estar autorizada para ter PDF disponível.";
                }
                
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao baixar PDF da nota fiscal {Id}. Status: {Status}, Response: {Response}", 
                    id, response.StatusCode, errorContent);
                TempData["ErrorMessage"] = "Erro ao baixar PDF da nota fiscal. Tente novamente ou entre em contato com o suporte.";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar PDF da nota fiscal {Id}", id);
            return StatusCode(500, "Erro ao baixar PDF da nota fiscal");
        }
    }

    // GET: NotaFiscal
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Buscando lista de notas fiscais...");
            
            // Verifica se há token na sessão
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token não encontrado na sessão - redirecionando para login");
                return RedirectToAction("Login", "Auth");
            }
            
            // Validação de parâmetros
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var resultado = await _apiService.GetAsync<PagedResultViewModel<NotaFiscalViewModel>>($"notafiscal?page={page}&pageSize={pageSize}");
            
            if (resultado == null)
            {
                _logger.LogWarning("Lista de notas fiscais retornou null - pode ser erro de autenticação ou lista vazia");
                return View(new PagedResultViewModel<NotaFiscalViewModel> 
                { 
                    Items = new List<NotaFiscalViewModel>(), 
                    PageNumber = page, 
                    PageSize = pageSize,
                    TotalCount = 0
                });
            }
            
            _logger.LogInformation("Encontradas {TotalCount} notas fiscais (página {Page} de {TotalPages})", 
                resultado.TotalCount, resultado.PageNumber, resultado.TotalPages);
            return View(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Não autorizado ao listar notas fiscais - redirecionando para login");
            TempData["ErrorMessage"] = "Sessão expirada. Faça login novamente.";
            return RedirectToAction("Login", "Auth");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar notas fiscais");
            ViewBag.ErrorMessage = "Erro ao carregar notas fiscais. Tente novamente.";
            return View(new PagedResultViewModel<NotaFiscalViewModel> 
            { 
                Items = new List<NotaFiscalViewModel>(), 
                PageNumber = page, 
                PageSize = pageSize,
                TotalCount = 0
            });
        }
    }

    // GET: NotaFiscal/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var nota = await _apiService.GetAsync<NotaFiscalViewModel>($"notafiscal/{id}");
            if (nota == null)
            {
                return NotFound();
            }
            return View(nota);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar nota fiscal {Id}", id);
            return NotFound();
        }
    }

    // GET: NotaFiscal/Create
    public async Task<IActionResult> Create()
    {
        try
        {
            // Busca todos os tomadores (usa pageSize grande para pegar todos)
            List<TomadorViewModel> tomadores = new List<TomadorViewModel>();
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar lista de tomadores");
            }

            var model = new NotaFiscalCreateViewModel
            {
                Tomadores = tomadores
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dados para criar nota fiscal");
            ViewBag.ErrorMessage = "Erro ao carregar dados. Tente novamente.";
            return View(new NotaFiscalCreateViewModel());
        }
    }

    // POST: NotaFiscal/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotaFiscalCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                model.Tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch { }
            return View(model);
        }

        try
        {
            // Cria um item de serviço padrão com base nos dados da nota fiscal
            // TODO: Implementar interface para múltiplos itens de serviço no futuro
            var itensServico = new List<object>
            {
                new
                {
                    codigoServico = !string.IsNullOrWhiteSpace(model.CodigoServico) ? model.CodigoServico : "1401",
                    discriminacao = model.DiscriminacaoServicos,
                    quantidade = 1,
                    valorUnitario = model.ValorServicos,
                    aliquotaIss = 0,
                    itemListaServico = !string.IsNullOrWhiteSpace(model.ItemListaServico) ? model.ItemListaServico : "14.01"
                }
            };

            var createData = new
            {
                tomadorId = model.TomadorId > 0 ? model.TomadorId : 0, // 0 = não identificado
                serie = !string.IsNullOrWhiteSpace(model.Serie) ? model.Serie : "900", // Padrão 900
                competencia = model.Competencia,
                valorServicos = model.ValorServicos,
                valorDeducoes = model.ValorDeducoes,
                valorPis = model.ValorPis,
                valorCofins = model.ValorCofins,
                valorCsll = model.ValorCsll,
                valorIr = model.ValorIr,
                valorIss = model.ValorIss,
                valorInss = model.ValorInss,
                discriminacaoServicos = model.DiscriminacaoServicos,
                codigoMunicipio = model.CodigoMunicipio ?? string.Empty, // Opcional - será preenchido pela empresa se vazio
                observacoes = model.Observacoes,
                itensServico = itensServico
            };

            var result = await _apiService.PostAsync<NotaFiscalViewModel>("notafiscal", createData);
            if (result != null)
            {
                TempData["SuccessMessage"] = "✅ Nota Fiscal criada com sucesso! Ela está em Rascunho e pode ser emitida quando estiver pronta.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "⚠️ Não foi possível criar a nota fiscal. Verifique os dados informados e tente novamente.");
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                model.Tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch { }
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar nota fiscal");
            
            var errorMessage = ex.Message.ToLower();
            string friendlyMessage;
            
            if (errorMessage.Contains("tomador") || errorMessage.Contains("não encontrado"))
            {
                friendlyMessage = "⚠️ Tomador não encontrado. Por favor, selecione um tomador válido.";
            }
            else if (errorMessage.Contains("código do município") || errorMessage.Contains("município"))
            {
                friendlyMessage = "⚠️ Código do município é obrigatório. Configure na empresa ou informe na nota fiscal.";
            }
            else if (errorMessage.Contains("serviço") || errorMessage.Contains("item"))
            {
                friendlyMessage = "⚠️ A nota fiscal deve ter pelo menos um item de serviço.";
            }
            else
            {
                friendlyMessage = "⚠️ Erro ao criar a nota fiscal. Verifique os dados e tente novamente.";
            }
            
            ModelState.AddModelError("", friendlyMessage);
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                model.Tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch { }
            return View(model);
        }
    }

    // POST: NotaFiscal/Cancelar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string motivo)
    {
        try
        {
            var cancelData = new { motivo = motivo };
            var success = await _apiService.PostAsync<object>($"notafiscal/{id}/cancelar", cancelData);
            if (success != null)
            {
                TempData["SuccessMessage"] = "✅ Nota Fiscal cancelada com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Não foi possível cancelar a nota fiscal. Verifique se ela pode ser cancelada e tente novamente.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal {Id}", id);
            
            var errorMessage = ex.Message.ToLower();
            if (errorMessage.Contains("já cancelada") || errorMessage.Contains("não pode ser cancelada"))
            {
                TempData["WarningMessage"] = $"ℹ️ {ex.Message}";
            }
            else if (errorMessage.Contains("não encontrada"))
            {
                TempData["ErrorMessage"] = "❌ Nota fiscal não encontrada.";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Erro ao cancelar a nota fiscal. Tente novamente ou entre em contato com o suporte.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: NotaFiscal/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var nota = await _apiService.GetAsync<NotaFiscalViewModel>($"notafiscal/{id}");
            if (nota == null)
            {
                return NotFound();
            }

            // Só permite editar notas em Rascunho
            if (nota.Situacao != 1) // 1 = Rascunho
            {
                var situacaoTexto = nota.Situacao switch
                {
                    2 => "Autorizada",
                    3 => "Cancelada",
                    4 => "Rejeitada",
                    _ => "Processada"
                };
                TempData["WarningMessage"] = $"ℹ️ Apenas notas em Rascunho podem ser editadas. Esta nota está com status: {situacaoTexto}.";
                return RedirectToAction(nameof(Index));
            }

            // Busca os itens de serviço para obter CodigoServico e ItemListaServico
            var notaCompleta = await _apiService.GetAsync<NotaFiscalViewModel>($"notafiscal/{id}");
            var primeiroItem = notaCompleta?.ItensServico?.FirstOrDefault();
            
            // Se não houver itens, usa valores padrão
            var codigoServico = primeiroItem?.CodigoServico ?? "1401";
            var itemListaServico = primeiroItem?.ItemListaServico ?? "14.01";

            // Busca todos os tomadores (usa pageSize grande para pegar todos)
            List<TomadorViewModel> tomadores = new List<TomadorViewModel>();
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar lista de tomadores para edição");
            }

            var model = new NotaFiscalCreateViewModel
            {
                TomadorId = nota.TomadorId,
                Serie = nota.Serie,
                Competencia = nota.Competencia,
                ValorServicos = nota.ValorServicos,
                ValorDeducoes = nota.ValorDeducoes,
                ValorPis = nota.ValorPis,
                ValorCofins = nota.ValorCofins,
                ValorCsll = nota.ValorCsll,
                ValorIr = nota.ValorIr,
                ValorIss = nota.ValorIss,
                ValorInss = nota.ValorInss,
                DiscriminacaoServicos = nota.DiscriminacaoServicos,
                CodigoServico = codigoServico,
                ItemListaServico = itemListaServico,
                CodigoMunicipio = nota.CodigoMunicipio,
                Observacoes = nota.Observacoes,
                Tomadores = tomadores
            };

            ViewBag.NotaId = id;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar nota fiscal {Id} para edição", id);
            TempData["ErrorMessage"] = "Erro ao carregar nota fiscal. Tente novamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: NotaFiscal/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NotaFiscalCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                model.Tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch { }
            ViewBag.NotaId = id;
            return View(model);
        }

        try
        {
            var itensServico = new List<object>
            {
                new
                {
                    codigoServico = !string.IsNullOrWhiteSpace(model.CodigoServico) ? model.CodigoServico : "1401",
                    discriminacao = model.DiscriminacaoServicos,
                    quantidade = 1,
                    valorUnitario = model.ValorServicos,
                    aliquotaIss = 0,
                    itemListaServico = !string.IsNullOrWhiteSpace(model.ItemListaServico) ? model.ItemListaServico : "14.01"
                }
            };

            var updateData = new
            {
                tomadorId = model.TomadorId > 0 ? model.TomadorId : 0, // Garante que seja 0 se não selecionado
                serie = model.Serie,
                competencia = model.Competencia,
                valorServicos = model.ValorServicos,
                valorDeducoes = model.ValorDeducoes,
                valorPis = model.ValorPis,
                valorCofins = model.ValorCofins,
                valorCsll = model.ValorCsll,
                valorIr = model.ValorIr,
                valorIss = model.ValorIss,
                valorInss = model.ValorInss,
                discriminacaoServicos = model.DiscriminacaoServicos,
                codigoMunicipio = model.CodigoMunicipio ?? string.Empty,
                observacoes = model.Observacoes,
                itensServico = itensServico
            };

            var result = await _apiService.PutAsync<NotaFiscalViewModel>($"notafiscal/{id}", updateData);
            if (result != null)
            {
                TempData["SuccessMessage"] = "✅ Nota Fiscal atualizada com sucesso!";
                
                // Recarrega os dados atualizados e mantém na tela de edição
                var notaAtualizada = await _apiService.GetAsync<NotaFiscalViewModel>($"notafiscal/{id}");
                if (notaAtualizada != null)
                {
                    var notaCompleta = await _apiService.GetAsync<NotaFiscalViewModel>($"notafiscal/{id}");
                    var primeiroItem = notaCompleta?.ItensServico?.FirstOrDefault();
                    var codigoServico = primeiroItem?.CodigoServico ?? "1401";
                    var itemListaServico = primeiroItem?.ItemListaServico ?? "14.01";
                    
                    model.TomadorId = notaAtualizada.TomadorId;
                    model.Serie = notaAtualizada.Serie;
                    model.Competencia = notaAtualizada.Competencia;
                    model.ValorServicos = notaAtualizada.ValorServicos;
                    model.ValorDeducoes = notaAtualizada.ValorDeducoes;
                    model.ValorPis = notaAtualizada.ValorPis;
                    model.ValorCofins = notaAtualizada.ValorCofins;
                    model.ValorCsll = notaAtualizada.ValorCsll;
                    model.ValorIr = notaAtualizada.ValorIr;
                    model.ValorIss = notaAtualizada.ValorIss;
                    model.ValorInss = notaAtualizada.ValorInss;
                    model.DiscriminacaoServicos = notaAtualizada.DiscriminacaoServicos;
                    model.CodigoServico = codigoServico;
                    model.ItemListaServico = itemListaServico;
                    model.CodigoMunicipio = notaAtualizada.CodigoMunicipio;
                    model.Observacoes = notaAtualizada.Observacoes;
                }
                
                try
                {
                    var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                    model.Tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
                }
                catch { }
                ViewBag.NotaId = id;
                return View(model);
            }

            ModelState.AddModelError("", "⚠️ Não foi possível atualizar a nota fiscal. Verifique os dados e tente novamente.");
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                model.Tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch { }
            ViewBag.NotaId = id;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar nota fiscal {Id}", id);
            ModelState.AddModelError("", $"Erro ao atualizar nota fiscal: {ex.Message}");
            try
            {
                var tomadoresPaginados = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>("tomador?page=1&pageSize=1000");
                model.Tomadores = tomadoresPaginados?.Items?.ToList() ?? new List<TomadorViewModel>();
            }
            catch { }
            ViewBag.NotaId = id;
            return View(model);
        }
    }

    // POST: NotaFiscal/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _apiService.DeleteAsync($"notafiscal/{id}");
            if (result)
            {
                TempData["SuccessMessage"] = "✅ Nota Fiscal excluída com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Não foi possível excluir a nota fiscal. Tente novamente.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir nota fiscal {Id}", id);
            
            var errorMessage = ex.Message.ToLower();
            if (errorMessage.Contains("não encontrada"))
            {
                TempData["WarningMessage"] = "ℹ️ A nota fiscal não foi encontrada ou já foi excluída.";
            }
            else if (errorMessage.Contains("não pode ser excluída") || errorMessage.Contains("já processada"))
            {
                TempData["WarningMessage"] = $"ℹ️ {ex.Message}";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Erro ao excluir a nota fiscal. Tente novamente.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: NotaFiscal/ConsultarStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConsultarStatus(int id)
    {
        try
        {
            var result = await _apiService.PostAsync<NotaFiscalViewModel>($"notafiscal/{id}/consultar-status", null);
            if (result != null)
            {
                var mensagem = "✅ Status da nota fiscal consultado e atualizado com sucesso!";
                if (!string.IsNullOrWhiteSpace(result.XMotivo))
                {
                    mensagem += $"<br/><strong>{result.XMotivo}</strong>";
                }
                TempData["SuccessMessage"] = mensagem;
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Não foi possível consultar o status da nota fiscal. Verifique se a nota possui nsNRec.";
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Erro HTTP ao consultar status da nota fiscal {Id}", id);
            
            var errorMessage = httpEx.Message.ToLower();
            if (errorMessage.Contains("nsnrec") || errorMessage.Contains("protocolo"))
            {
                TempData["ErrorMessage"] = "⚠️ Esta nota fiscal não possui número de protocolo (nsNRec) para consultar status.";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Erro ao consultar status da nota fiscal. Tente novamente.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status da nota fiscal {Id}", id);
            TempData["ErrorMessage"] = "⚠️ Erro ao consultar status da nota fiscal. Tente novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: NotaFiscal/Emitir/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Emitir(int id)
    {
        try
        {
            var success = await _apiService.PostAsync<NotaFiscalViewModel>($"notafiscal/{id}/emitir", null);
            if (success != null)
            {
                // Só exibe mensagem de sucesso se a nota estiver realmente autorizada
                if (success.Situacao == 2) // 2 = Autorizada
                {
                    var numeroNota = !string.IsNullOrEmpty(success.Numero) ? $" Número: {success.Numero}" : "";
                    var codigoVerificacao = !string.IsNullOrEmpty(success.CodigoVerificacao) ? $" Código de Verificação: {success.CodigoVerificacao}" : "";
                    
                    TempData["SuccessMessage"] = $"✅ Nota Fiscal Autorizada com Sucesso!{numeroNota}{codigoVerificacao}";
                    
                    // Sempre armazena o ID da nota para mostrar diálogo de visualização (PDF será baixado via API)
                    TempData["ShowPDFDialogId"] = id.ToString();
                }
                else if (success.Situacao == 1) // 1 = Rascunho (enviado para Sefaz, aguardando processamento)
                {
                    TempData["InfoMessage"] = "📤 Nota Fiscal enviada para processamento. Aguarde alguns instantes e verifique o status na lista.";
                }
                else
                {
                    var numeroNota = !string.IsNullOrEmpty(success.Numero) ? $" Número: {success.Numero}" : "";
                    TempData["InfoMessage"] = $"ℹ️ Nota Fiscal processada. Status: {FormatarSituacao(success.Situacao)}{numeroNota}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Não foi possível processar a emissão da nota fiscal. Verifique os dados e tente novamente.";
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Erro HTTP ao emitir nota fiscal {Id}", id);
            
            // Mensagens amigáveis baseadas no tipo de erro
            var errorMessage = httpEx.Message.ToLower();
            if (errorMessage.Contains("certificado digital") || errorMessage.Contains("certificado não cadastrado"))
            {
                TempData["ErrorMessage"] = "🔐 Certificado Digital não encontrado. Por favor, cadastre um certificado A1 válido no perfil da empresa antes de emitir notas fiscais.";
            }
            else if (errorMessage.Contains("certificado") && errorMessage.Contains("senha"))
            {
                TempData["ErrorMessage"] = "🔐 Erro ao validar o certificado digital. Verifique se a senha está correta e se o certificado não está expirado.";
            }
            else if (errorMessage.Contains("401") || errorMessage.Contains("unauthorized"))
            {
                TempData["ErrorMessage"] = "🔑 Erro de autenticação na API Nacional. Verifique as credenciais configuradas.";
            }
            else if (errorMessage.Contains("404") || errorMessage.Contains("not found"))
            {
                TempData["ErrorMessage"] = "❌ Nota fiscal não encontrada ou já foi processada anteriormente.";
            }
            else if (errorMessage.Contains("timeout") || errorMessage.Contains("timed out"))
            {
                TempData["ErrorMessage"] = "⏱️ A requisição demorou muito para responder. A nota fiscal pode ter sido processada. Verifique o status na lista.";
            }
            else if (errorMessage.Contains("500") || errorMessage.Contains("internal server"))
            {
                TempData["ErrorMessage"] = "🔧 Erro no servidor da API Nacional. Tente novamente em alguns instantes ou entre em contato com o suporte.";
            }
            else
            {
                TempData["ErrorMessage"] = $"⚠️ Erro ao comunicar com a API Nacional: {httpEx.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal {Id}", id);
            
            var errorMessage = ex.Message.ToLower();
            
            // Mensagens específicas para diferentes tipos de erro
            if (errorMessage.Contains("certificado digital não cadastrado"))
            {
                TempData["ErrorMessage"] = "🔐 Certificado Digital não cadastrado. Por favor, cadastre um certificado A1 válido no perfil da empresa.";
            }
            else if (errorMessage.Contains("certificado não possui chave privada"))
            {
                TempData["ErrorMessage"] = "🔐 O certificado digital não possui chave privada. Certifique-se de usar um certificado A1 (.pfx ou .p12) válido.";
            }
            else if (errorMessage.Contains("já foi processada") || errorMessage.Contains("situação atual"))
            {
                TempData["WarningMessage"] = $"ℹ️ {ex.Message}";
            }
            else if (errorMessage.Contains("não encontrada"))
            {
                TempData["ErrorMessage"] = "❌ Nota fiscal não encontrada. Verifique se a nota ainda existe.";
            }
            else if (errorMessage.Contains("xml") || errorMessage.Contains("assinatura"))
            {
                TempData["ErrorMessage"] = "📝 Erro ao gerar ou assinar o XML da nota fiscal. Verifique o certificado digital e tente novamente.";
            }
            else
            {
                // Mensagem genérica mas amigável
                TempData["ErrorMessage"] = $"⚠️ Ocorreu um erro ao emitir a nota fiscal. Detalhes: {ex.Message}";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private string FormatarSituacao(int situacao)
    {
        return situacao switch
        {
            1 => "Rascunho",
            2 => "Autorizada",
            3 => "Cancelada",
            4 => "Rejeitada",
            5 => "Enviada",
            _ => "Desconhecida"
        };
    }

    // POST: NotaFiscal/ReverterParaRascunho/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReverterParaRascunho(int id)
    {
        try
        {
            var result = await _apiService.PostAsync<NotaFiscalViewModel>($"notafiscal/{id}/reverter-para-rascunho", null);
            if (result != null)
            {
                TempData["SuccessMessage"] = "✅ Nota Fiscal revertida para Rascunho! Agora você pode editá-la e tentar emitir novamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Não foi possível reverter a nota fiscal. Verifique se ela pode ser revertida e tente novamente.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reverter nota fiscal {Id} para Rascunho", id);
            
            var errorMessage = ex.Message.ToLower();
            if (errorMessage.Contains("não é possível reverter") || errorMessage.Contains("apenas notas rejeitadas"))
            {
                TempData["WarningMessage"] = $"ℹ️ {ex.Message}";
            }
            else if (errorMessage.Contains("não encontrada"))
            {
                TempData["ErrorMessage"] = "❌ Nota fiscal não encontrada.";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Erro ao reverter a nota fiscal. Tente novamente.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: NotaFiscal/Copiar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copiar(int id)
    {
        try
        {
            var result = await _apiService.PostAsync<NotaFiscalViewModel>($"notafiscal/{id}/copiar", null);
            if (result != null)
            {
                TempData["SuccessMessage"] = $"✅ Nota Fiscal copiada com sucesso! Nova nota fiscal #{result.Id} criada em Rascunho.";
            }
            else
            {
                TempData["ErrorMessage"] = "⚠️ Não foi possível copiar a nota fiscal. Tente novamente.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao copiar nota fiscal {Id}", id);
            
            var errorMessage = ex.Message.ToLower();
            if (errorMessage.Contains("não encontrada"))
            {
                TempData["ErrorMessage"] = "❌ Nota fiscal não encontrada.";
            }
            else
            {
                TempData["ErrorMessage"] = $"⚠️ Erro ao copiar a nota fiscal: {ex.Message}";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: NotaFiscal/EnviarEmail/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarEmail(int id)
    {
        try
        {
            // Verifica se há token na sessão
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Sessão expirada. Faça login novamente." });
            }

            // Faz a requisição para a API usando HttpClient com o token
            using var httpClient = new HttpClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5215";
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.PostAsync($"{apiBaseUrl}/api/notafiscal/{id}/enviar-email", null);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var responseJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    var message = responseJson?.ContainsKey("message") == true 
                        ? responseJson["message"]?.ToString() 
                        : "PDF enviado por email com sucesso!";
                    return Json(new { success = true, message = message });
                }
                catch
                {
                    return Json(new { success = true, message = "PDF enviado por email com sucesso!" });
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = "Erro ao enviar PDF por email.";
                
                try
                {
                    var errorJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                    if (errorJson != null && errorJson.ContainsKey("error"))
                    {
                        errorMessage = errorJson["error"]?.ToString() ?? errorMessage;
                    }
                }
                catch { }
                
                return Json(new { success = false, message = errorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar PDF por email da nota fiscal {Id}", id);
            return Json(new { success = false, message = $"Erro ao enviar PDF por email: {ex.Message}" });
        }
    }
}

