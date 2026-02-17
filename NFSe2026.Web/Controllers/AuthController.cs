using Microsoft.AspNetCore.Mvc;
using NFSe2026.Web.Models;
using NFSe2026.Web.Services;
using System.Text;
using System.Text.Json;

namespace NFSe2026.Web.Controllers;

public class AuthController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApiService apiService, ILogger<AuthController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Se já estiver logado, redireciona para dashboard
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var loginData = new
            {
                email = model.Email,
                senha = model.Senha
            };

            var response = await _apiService.PostAsync<LoginResponseModel>("auth/login", loginData);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Salva o token na sessão
                HttpContext.Session.SetString("JWTToken", response.Token);
                HttpContext.Session.SetString("UsuarioNome", response.Usuario.Nome);
                HttpContext.Session.SetString("UsuarioEmail", response.Usuario.Email);
                HttpContext.Session.SetString("EmpresaId", response.Empresa.Id.ToString());
                HttpContext.Session.SetString("EmpresaRazaoSocial", response.Empresa.RazaoSocial);

                _logger.LogInformation("Usuário {Email} fez login com sucesso", model.Email);
                TempData["SuccessMessage"] = $"Bem-vindo, {response.Usuario.Nome}!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email ou senha inválidos");
            return View(model);
        }
        catch (System.Net.Http.HttpRequestException ex) when (ex.Message.Contains("recusou") || ex.Message.Contains("refused") || ex.Message.Contains("5215"))
        {
            _logger.LogError(ex, "Erro ao conectar com a API");
            ModelState.AddModelError("", "Não foi possível conectar à API. Verifique se a API está rodando na porta 5215. Consulte o arquivo COMO_INICIAR.md para mais informações.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login");
            ModelState.AddModelError("", $"Erro ao realizar login: {ex.Message}. Tente novamente.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Cadastro()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(CadastroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Converte o arquivo de logotipo para Base64 se fornecido
            string? logotipoBase64 = null;
            if (model.LogotipoFile != null && model.LogotipoFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await model.LogotipoFile.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                logotipoBase64 = Convert.ToBase64String(fileBytes);
            }

            var cadastroData = new
            {
                cnpj = model.CNPJ,
                nome = model.Nome,
                email = model.Email,
                senha = model.Senha,
                telefone = model.Telefone,
                logotipo = logotipoBase64
            };

            var response = await _apiService.PostAsync<LoginResponseModel>("auth/cadastro", cadastroData);

            if (response != null)
            {
                // Se o token estiver vazio, significa que precisa validar o email
                if (string.IsNullOrEmpty(response.Token))
                {
                    // Salva o email na sessão para a página de validação
                    HttpContext.Session.SetString("EmailParaValidar", model.Email);
                    TempData["InfoMessage"] = $"Um código de validação foi enviado para {model.Email}. Por favor, verifique sua caixa de entrada.";
                    return RedirectToAction("ValidarEmail");
                }
                else
                {
                    // Se já tem token, faz login direto
                    HttpContext.Session.SetString("JWTToken", response.Token);
                    HttpContext.Session.SetString("UsuarioNome", response.Usuario.Nome);
                    HttpContext.Session.SetString("UsuarioEmail", response.Usuario.Email);
                    HttpContext.Session.SetString("EmpresaId", response.Empresa.Id.ToString());
                    HttpContext.Session.SetString("EmpresaRazaoSocial", response.Empresa.RazaoSocial);

                    _logger.LogInformation("Empresa {CNPJ} cadastrada com sucesso", model.CNPJ);
                    TempData["SuccessMessage"] = $"Bem-vindo, {response.Usuario.Nome}! Cadastro realizado com sucesso.";
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Erro ao cadastrar empresa. Verifique os dados informados.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar empresa");
            ModelState.AddModelError("", "Erro ao realizar cadastro. Tente novamente.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult EsqueciSenha()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EsqueciSenha(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ViewBag.ErrorMessage = "Por favor, informe seu email.";
            return View();
        }

        try
        {
            // TODO: Implementar chamada à API para recuperação de senha
            // var result = await _apiService.PostAsync<object>("auth/esqueci-senha", new { email });
            
            ViewBag.SuccessMessage = "Se o email informado estiver cadastrado, você receberá instruções para redefinir sua senha.";
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar recuperação de senha para {Email}", email);
            ViewBag.ErrorMessage = "Erro ao processar solicitação. Tente novamente mais tarde.";
            return View();
        }
    }

    [HttpGet]
    public IActionResult ValidarEmail()
    {
        var email = HttpContext.Session.GetString("EmailParaValidar");
        if (string.IsNullOrEmpty(email))
        {
            // Se não tem email na sessão, redireciona para cadastro
            return RedirectToAction("Cadastro");
        }

        var model = new ValidarEmailViewModel
        {
            Email = email
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidarEmail(ValidarEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var validarData = new
            {
                email = model.Email,
                codigo = model.Codigo
            };

            var response = await _apiService.PostAsync<LoginResponseModel>("auth/validar-email", validarData);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Remove o email da sessão
                HttpContext.Session.Remove("EmailParaValidar");

                // Salva o token na sessão
                HttpContext.Session.SetString("JWTToken", response.Token);
                HttpContext.Session.SetString("UsuarioNome", response.Usuario.Nome);
                HttpContext.Session.SetString("UsuarioEmail", response.Usuario.Email);
                HttpContext.Session.SetString("EmpresaId", response.Empresa.Id.ToString());
                HttpContext.Session.SetString("EmpresaRazaoSocial", response.Empresa.RazaoSocial);

                _logger.LogInformation("Email {Email} validado com sucesso", model.Email);
                TempData["SuccessMessage"] = $"Bem-vindo, {response.Usuario.Nome}! Email validado com sucesso.";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Erro ao validar código. Verifique o código informado.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar email");
            ModelState.AddModelError("", $"Erro ao validar código: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
