using Microsoft.AspNetCore.Mvc;
using NFSe2026.Web.Models;
using NFSe2026.Web.Services;

namespace NFSe2026.Web.Controllers;

public class PrestadorController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<PrestadorController> _logger;

    public PrestadorController(ApiService apiService, ILogger<PrestadorController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // GET: Prestador
    public async Task<IActionResult> Index()
    {
        try
        {
            var prestadores = await _apiService.GetAsync<List<PrestadorViewModel>>("prestador");
            return View(prestadores ?? new List<PrestadorViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar prestadores");
            ViewBag.ErrorMessage = "Erro ao carregar prestadores. Tente novamente.";
            return View(new List<PrestadorViewModel>());
        }
    }

    // GET: Prestador/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var prestador = await _apiService.GetAsync<PrestadorViewModel>($"prestador/{id}");
            if (prestador == null)
            {
                return NotFound();
            }
            return View(prestador);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar prestador {Id}", id);
            return NotFound();
        }
    }

    // GET: Prestador/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Prestador/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrestadorCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiService.PostAsync<PrestadorViewModel>("prestador", model);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Prestador cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Erro ao cadastrar prestador. Tente novamente.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar prestador");
            ModelState.AddModelError("", "Erro ao cadastrar prestador. Tente novamente.");
            return View(model);
        }
    }

    // GET: Prestador/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var prestador = await _apiService.GetAsync<PrestadorViewModel>($"prestador/{id}");
            if (prestador == null)
            {
                return NotFound();
            }

            var editModel = new PrestadorCreateViewModel
            {
                RazaoSocial = prestador.RazaoSocial,
                NomeFantasia = prestador.NomeFantasia,
                CNPJ = prestador.CNPJ,
                InscricaoMunicipal = prestador.InscricaoMunicipal,
                Endereco = prestador.Endereco,
                Cidade = prestador.Cidade,
                UF = prestador.UF,
                CEP = prestador.CEP,
                Telefone = prestador.Telefone,
                Email = prestador.Email,
                Ambiente = prestador.Ambiente,
                Ativo = prestador.Ativo
            };
            
            ViewBag.Id = prestador.Id;

            return View(editModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar prestador {Id} para edição", id);
            return NotFound();
        }
    }

    // POST: Prestador/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PrestadorCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var success = await _apiService.PutAsync($"prestador/{id}", model);
            if (success)
            {
                TempData["SuccessMessage"] = "Prestador atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Erro ao atualizar prestador. Tente novamente.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar prestador {Id}", id);
            ModelState.AddModelError("", "Erro ao atualizar prestador. Tente novamente.");
            return View(model);
        }
    }

    // GET: Prestador/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var prestador = await _apiService.GetAsync<PrestadorViewModel>($"prestador/{id}");
            if (prestador == null)
            {
                return NotFound();
            }
            return View(prestador);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar prestador {Id} para exclusão", id);
            return NotFound();
        }
    }

    // POST: Prestador/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var success = await _apiService.DeleteAsync($"prestador/{id}");
            if (success)
            {
                TempData["SuccessMessage"] = "Prestador excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Erro ao excluir prestador. Tente novamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir prestador {Id}", id);
            TempData["ErrorMessage"] = "Erro ao excluir prestador. Tente novamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}

