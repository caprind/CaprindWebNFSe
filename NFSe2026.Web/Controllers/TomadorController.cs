using Microsoft.AspNetCore.Mvc;
using NFSe2026.Web.Models;
using NFSe2026.Web.Services;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace NFSe2026.Web.Controllers;

public class TomadorController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<TomadorController> _logger;

    public TomadorController(ApiService apiService, ILogger<TomadorController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // GET: Tomador
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Buscando lista de tomadores... Página: {Page}, Tamanho: {PageSize}", page, pageSize);
            
            // Validação de parâmetros
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var resultado = await _apiService.GetAsync<PagedResultViewModel<TomadorViewModel>>($"tomador?page={page}&pageSize={pageSize}");
            
            if (resultado == null)
            {
                _logger.LogWarning("Lista de tomadores retornou null - pode ser erro de autenticação ou lista vazia");
                
                // Verifica se há token na sessão
                var token = HttpContext.Session.GetString("JWTToken");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token não encontrado na sessão - redirecionando para login");
                    return RedirectToAction("Login", "Auth");
                }
                
                // Se tem token mas retornou null, pode ser lista vazia ou erro
                // Retorna lista vazia (o usuário pode não ter tomadores cadastrados)
                return View(new PagedResultViewModel<TomadorViewModel> 
                { 
                    Items = new List<TomadorViewModel>(), 
                    PageNumber = page, 
                    PageSize = pageSize,
                    TotalCount = 0
                });
            }
            
            _logger.LogInformation("Encontrados {TotalCount} tomadores (página {Page} de {TotalPages})", 
                resultado.TotalCount, resultado.PageNumber, resultado.TotalPages);
            return View(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Não autorizado ao listar tomadores - redirecionando para login");
            TempData["ErrorMessage"] = "Sessão expirada. Faça login novamente.";
            return RedirectToAction("Login", "Auth");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar tomadores");
            ViewBag.ErrorMessage = "Erro ao carregar tomadores. Tente novamente.";
            return View(new PagedResultViewModel<TomadorViewModel> 
            { 
                Items = new List<TomadorViewModel>(), 
                PageNumber = page, 
                PageSize = pageSize,
                TotalCount = 0
            });
        }
    }

    // GET: Tomador/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var tomador = await _apiService.GetAsync<TomadorViewModel>($"tomador/{id}");
            if (tomador == null)
            {
                return NotFound();
            }
            return View(tomador);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tomador {Id}", id);
            return NotFound();
        }
    }

    // GET: Tomador/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Tomador/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TomadorCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiService.PostAsync<TomadorViewModel>("tomador", model);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Tomador cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Erro ao cadastrar tomador. Tente novamente.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar tomador");
            ModelState.AddModelError("", "Erro ao cadastrar tomador. Tente novamente.");
            return View(model);
        }
    }

    // GET: Tomador/CreatePorCNPJ
    public IActionResult CreatePorCNPJ()
    {
        return View();
    }

    // POST: Tomador/CreatePorCNPJ
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePorCNPJ(TomadorPorCNPJViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var cadastroData = new { cnpj = model.CNPJ };
            var result = await _apiService.PostAsync<TomadorViewModel>("tomador/por-cnpj", cadastroData);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Tomador cadastrado com sucesso a partir do CNPJ!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Erro ao cadastrar tomador. Verifique se o CNPJ está correto.");
            return View(model);
        }
        catch (HttpRequestException ex)
        {
            // Captura erros HTTP (incluindo 409 Conflict)
            var errorMessage = ex.Message;
            
            // Verifica se é erro de conflito (CNPJ já existe)
            if (errorMessage.Contains("409") || errorMessage.Contains("Conflict"))
            {
                // Tenta encontrar o tomador existente
                try
                {
                    var cnpj = model.CNPJ.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
                    var tomadores = await _apiService.GetAsync<List<TomadorViewModel>>("tomador");
                    var tomadorExistente = tomadores?.FirstOrDefault(t => t.CPFCNPJ == cnpj);
                    
                    if (tomadorExistente != null)
                    {
                        TempData["ErrorMessage"] = $"Já existe um tomador cadastrado com este CNPJ: {tomadorExistente.RazaoSocialNome}";
                        return RedirectToAction(nameof(Details), new { id = tomadorExistente.Id });
                    }
                }
                catch { }
                
                ModelState.AddModelError("", "Já existe um tomador cadastrado com este CNPJ. Verifique a lista de tomadores.");
            }
            else if (errorMessage.Contains("400") || errorMessage.Contains("BadRequest"))
            {
                // Extrai mensagem de erro mais específica
                if (errorMessage.Contains("CNPJ inválido"))
                {
                    ModelState.AddModelError("", "CNPJ inválido. Deve conter 14 dígitos.");
                }
                else if (errorMessage.Contains("não foi possível consultar"))
                {
                    ModelState.AddModelError("", "Não foi possível consultar os dados do CNPJ. Verifique se o CNPJ está correto.");
                }
                else
                {
                    ModelState.AddModelError("", errorMessage.Replace("Erro 400: ", "").Replace("Erro ao fazer POST tomador/por-cnpj: 400 ", ""));
                }
            }
            else
            {
                ModelState.AddModelError("", errorMessage.Replace("Erro ao fazer POST tomador/por-cnpj: ", ""));
            }
            
            _logger.LogWarning("Erro ao cadastrar tomador por CNPJ: {Error}", errorMessage);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar tomador por CNPJ");
            ModelState.AddModelError("", "Erro ao cadastrar tomador. Tente novamente.");
            return View(model);
        }
    }

    // GET: Tomador/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }
        
        try
        {
            var tomador = await _apiService.GetAsync<TomadorViewModel>($"tomador/{id}");
            if (tomador == null)
            {
                return NotFound();
            }

            var editModel = new TomadorCreateViewModel
            {
                TipoPessoa = tomador.TipoPessoa,
                CPFCNPJ = tomador.CPFCNPJ,
                RazaoSocialNome = tomador.RazaoSocialNome,
                InscricaoEstadual = tomador.InscricaoEstadual,
                InscricaoMunicipal = tomador.InscricaoMunicipal,
                Endereco = tomador.Endereco,
                Numero = tomador.Numero,
                Complemento = tomador.Complemento,
                Bairro = tomador.Bairro,
                Cidade = tomador.Cidade,
                UF = tomador.UF,
                CEP = tomador.CEP,
                Email = tomador.Email,
                Telefone = tomador.Telefone
            };

            return View(editModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tomador {Id} para edição", id);
            return NotFound();
        }
    }

    // POST: Tomador/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TomadorCreateViewModel model)
    {
        _logger.LogInformation("═══════════════════════════════════════════════════════");
        _logger.LogInformation("TOMADOR EDIT POST - RECEBIDO! ID: {Id}", id);
        _logger.LogInformation("═══════════════════════════════════════════════════════");
        
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            _logger.LogWarning("TOMADOR EDIT POST - Não autenticado, redirecionando para login");
            return RedirectToAction("Login", "Auth");
        }
        
        _logger.LogInformation("TOMADOR EDIT POST - Model recebido:");
        _logger.LogInformation("  - TipoPessoa: {TipoPessoa}", model?.TipoPessoa ?? 0);
        _logger.LogInformation("  - RazaoSocialNome: '{RazaoSocialNome}'", model?.RazaoSocialNome ?? "null");
        _logger.LogInformation("  - Email: '{Email}'", model?.Email ?? "null");
        _logger.LogInformation("  - Telefone: '{Telefone}'", model?.Telefone ?? "null");
        _logger.LogInformation("  - Endereco: '{Endereco}'", model?.Endereco ?? "null");
        
        // Remove erros do ModelState para campos que não estão no formulário de edição
        // - 'id' vem da rota, não precisa estar no modelo
        // - 'CPFCNPJ' não é editável, não está no formulário de edição
        if (ModelState.ContainsKey("id"))
        {
            ModelState.Remove("id");
        }
        if (ModelState.ContainsKey("CPFCNPJ"))
        {
            ModelState.Remove("CPFCNPJ");
        }
        
        // Log de erros do ModelState
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("TOMADOR EDIT POST - ModelState inválido!");
            foreach (var error in ModelState)
            {
                if (error.Value.Errors.Count > 0)
                {
                    _logger.LogWarning("  Campo '{Key}': {Errors}", error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }
            return View(model);
        }
        
        _logger.LogInformation("TOMADOR EDIT POST - ModelState válido, prosseguindo...");

        try
        {
            _logger.LogInformation("TOMADOR EDIT - Iniciando atualização. ID: {Id}, Email recebido: '{Email}', Telefone recebido: '{Telefone}'", 
                id, model.Email ?? "null", model.Telefone ?? "null");

            // Cria objeto com apenas os campos que a API espera (TomadorUpdateDTO não tem CPFCNPJ)
            var updateData = new
            {
                tipoPessoa = model.TipoPessoa,
                razaoSocialNome = model.RazaoSocialNome,
                inscricaoEstadual = model.InscricaoEstadual,
                inscricaoMunicipal = model.InscricaoMunicipal,
                endereco = model.Endereco,
                numero = model.Numero,
                complemento = model.Complemento,
                bairro = model.Bairro,
                cidade = model.Cidade,
                uf = model.UF,
                cep = model.CEP,
                email = model.Email, // Inclui mesmo se null
                telefone = model.Telefone // Inclui mesmo se null
            };

            _logger.LogInformation("TOMADOR EDIT - Dados a serem enviados para API: Email='{Email}', Telefone='{Telefone}'", 
                updateData.email ?? "null", updateData.telefone ?? "null");

            var success = await _apiService.PutAsync($"tomador/{id}", updateData);
            
            if (success)
            {
                _logger.LogInformation("TOMADOR EDIT - Atualização bem-sucedida. ID: {Id}", id);
                TempData["SuccessMessage"] = "Tomador atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("TOMADOR EDIT - Atualização retornou false. ID: {Id}", id);
            ModelState.AddModelError("", "Erro ao atualizar tomador. Tente novamente.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tomador {Id}", id);
            ModelState.AddModelError("", "Erro ao atualizar tomador. Tente novamente.");
            return View(model);
        }
    }

    // GET: Tomador/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var tomador = await _apiService.GetAsync<TomadorViewModel>($"tomador/{id}");
            if (tomador == null)
            {
                return NotFound();
            }
            return View(tomador);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tomador {Id} para exclusão", id);
            return NotFound();
        }
    }

    // POST: Tomador/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var success = await _apiService.DeleteAsync($"tomador/{id}");
            if (success)
            {
                TempData["SuccessMessage"] = "Tomador excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao excluir tomador. Tente novamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir tomador {Id}", id);
            TempData["ErrorMessage"] = "Erro ao excluir tomador. Tente novamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}

