using Microsoft.AspNetCore.Mvc;
using NFSe2026.Web.Models;

namespace NFSe2026.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.UsuarioNome = HttpContext.Session.GetString("UsuarioNome");
        ViewBag.EmpresaRazaoSocial = HttpContext.Session.GetString("EmpresaRazaoSocial");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
