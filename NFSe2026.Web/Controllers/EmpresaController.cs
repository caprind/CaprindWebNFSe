using Microsoft.AspNetCore.Mvc;
using NFSe2026.Web.Models;
using NFSe2026.Web.Services;

namespace NFSe2026.Web.Controllers;

public class EmpresaController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<EmpresaController> _logger;

    public EmpresaController(ApiService apiService, ILogger<EmpresaController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // GET: Empresa/Edit
    public async Task<IActionResult> Edit()
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var empresa = await _apiService.GetAsync<EmpresaViewModel>("empresa/meus-dados");
            if (empresa == null)
            {
                TempData["ErrorMessage"] = "Empresa não encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var model = new EmpresaEditViewModel
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
                LogotipoAtual = empresa.Logotipo,
                DataVencimentoCertificado = empresa.DataVencimentoCertificado,
                TemCertificadoDigital = empresa.TemCertificadoDigital,
                TemClientIdSecret = empresa.TemClientIdSecret,
                ProvedorNFSe = empresa.ProvedorNFSe
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados da empresa");
            TempData["ErrorMessage"] = "Erro ao carregar dados da empresa. Tente novamente.";
            return RedirectToAction("Index", "Home");
        }
    }

    // POST: Empresa/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmpresaEditViewModel model)
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Processa arquivo de logotipo se fornecido
            string? logotipoBase64 = model.Logotipo;
            if (Request.Form.Files.Count > 0)
            {
                var logotipoFile = Request.Form.Files["logotipoFile"];
                if (logotipoFile != null && logotipoFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await logotipoFile.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    logotipoBase64 = Convert.ToBase64String(fileBytes);
                }
            }

            // Se não foi fornecido novo logotipo, mantém o atual
            if (string.IsNullOrEmpty(logotipoBase64) && !string.IsNullOrEmpty(model.LogotipoAtual))
            {
                logotipoBase64 = model.LogotipoAtual;
            }

            var updateData = new
            {
                nomeFantasia = model.NomeFantasia,
                inscricaoEstadual = model.InscricaoEstadual,
                inscricaoMunicipal = model.InscricaoMunicipal,
                endereco = model.Endereco,
                numero = model.Numero,
                complemento = model.Complemento,
                bairro = model.Bairro,
                cidade = model.Cidade,
                uf = model.UF,
                codigoMunicipio = model.CodigoMunicipio,
                cep = model.CEP,
                telefone = model.Telefone,
                email = model.Email,
                logotipo = logotipoBase64,
                provedorNFSe = model.ProvedorNFSe
            };

            var result = await _apiService.PutAsync<EmpresaViewModel>("empresa/meus-dados", updateData);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Dados da empresa atualizados com sucesso!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Erro ao atualizar dados da empresa. Tente novamente.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa");
            ModelState.AddModelError("", $"Erro ao atualizar dados da empresa: {ex.Message}");
            return View(model);
        }
    }

    // GET: Empresa/CertificadoDigital
    public async Task<IActionResult> CertificadoDigital()
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var empresa = await _apiService.GetAsync<EmpresaViewModel>("empresa/meus-dados");
            if (empresa == null)
            {
                TempData["ErrorMessage"] = "Empresa não encontrada.";
                return RedirectToAction("Edit");
            }

            // Garante que o modelo tenha valores padrão para evitar NullReferenceException
            if (empresa.DataVencimentoCertificado == null && empresa.TemCertificadoDigital)
            {
                // Se tem certificado mas não tem data, mantém null (será tratado na view)
                _logger.LogWarning("Empresa tem certificado digital mas não tem data de vencimento cadastrada.");
            }

            return View(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados da empresa");
            TempData["ErrorMessage"] = "Erro ao carregar dados da empresa. Tente novamente.";
            return RedirectToAction("Edit");
        }
    }

    // POST: Empresa/CertificadoDigital
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CertificadoDigital(IFormFile certificadoFile, string senhaCertificado)
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            // Busca dados da empresa para passar para a view em caso de erro
            var empresaAtual = await _apiService.GetAsync<EmpresaViewModel>("empresa/meus-dados");
            
            if (certificadoFile == null || certificadoFile.Length == 0)
            {
                ModelState.AddModelError("", "O arquivo do certificado digital é obrigatório.");
                return View(empresaAtual);
            }

            if (string.IsNullOrWhiteSpace(senhaCertificado))
            {
                ModelState.AddModelError("", "A senha do certificado é obrigatória.");
                return View(empresaAtual);
            }

            // Valida extensão do arquivo
            var extensao = Path.GetExtension(certificadoFile.FileName).ToLower();
            if (extensao != ".pfx" && extensao != ".p12")
            {
                ModelState.AddModelError("", "O arquivo deve ser um certificado digital (.pfx ou .p12).");
                return View(empresaAtual);
            }

            // Converte o arquivo para Base64
            using var memoryStream = new MemoryStream();
            await certificadoFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var certificadoBase64 = Convert.ToBase64String(fileBytes);

            var certificadoData = new
            {
                certificadoDigital = certificadoBase64,
                senhaCertificado = senhaCertificado
            };

            var result = await _apiService.PostAsync<object>("empresa/certificado-digital", certificadoData);
            if (result != null)
            {
                // Busca os dados atualizados para mostrar a data de vencimento
                var empresaAtualizada = await _apiService.GetAsync<EmpresaViewModel>("empresa/meus-dados");
                if (empresaAtualizada != null && empresaAtualizada.DataVencimentoCertificado.HasValue)
                {
                    var dataVencimento = empresaAtualizada.DataVencimentoCertificado.Value.ToString("dd/MM/yyyy");
                    TempData["SuccessMessage"] = $"Certificado digital cadastrado com sucesso! Data de vencimento: {dataVencimento}";
                }
                else
                {
                    TempData["SuccessMessage"] = "Certificado digital cadastrado com sucesso!";
                }
                return RedirectToAction("CertificadoDigital");
            }

            ModelState.AddModelError("", "Erro ao cadastrar certificado digital. Tente novamente.");
            return View(empresaAtual);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar certificado digital");
            ModelState.AddModelError("", $"Erro ao cadastrar certificado digital: {ex.Message}");
            return View();
        }
    }

    // POST: Empresa/RemoverCertificadoDigital
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverCertificadoDigital()
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var success = await _apiService.DeleteAsync("empresa/certificado-digital");
            if (success)
            {
                TempData["SuccessMessage"] = "Certificado digital removido com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao remover certificado digital. Tente novamente.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover certificado digital");
            TempData["ErrorMessage"] = $"Erro ao remover certificado digital: {ex.Message}";
        }

        return RedirectToAction("Edit");
    }

    // GET: Empresa/CredenciaisAPI
    public async Task<IActionResult> CredenciaisAPI()
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var empresa = await _apiService.GetAsync<EmpresaViewModel>("empresa/meus-dados");
            if (empresa == null)
            {
                TempData["ErrorMessage"] = "Empresa não encontrada.";
                return RedirectToAction("Edit");
            }

            return View(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados da empresa");
            TempData["ErrorMessage"] = "Erro ao carregar dados da empresa. Tente novamente.";
            return RedirectToAction("Edit");
        }
    }

    // POST: Empresa/CredenciaisAPI
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CredenciaisAPI(string clientId, string clientSecret)
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            // Busca dados da empresa para passar para a view em caso de erro
            var empresaAtual = await _apiService.GetAsync<EmpresaViewModel>("empresa/meus-dados");
            
            if (string.IsNullOrWhiteSpace(clientId))
            {
                ModelState.AddModelError("", "O ClientId é obrigatório.");
                return View(empresaAtual);
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                ModelState.AddModelError("", "O ClientSecret é obrigatório.");
                return View(empresaAtual);
            }

            var credenciaisData = new
            {
                clientId = clientId,
                clientSecret = clientSecret
            };

            var result = await _apiService.PostAsync<object>("empresa/credenciais-api", credenciaisData);
            if (result != null)
            {
                TempData["SuccessMessage"] = "✅ Credenciais da API cadastradas com sucesso!";
                return RedirectToAction("CredenciaisAPI");
            }

            ModelState.AddModelError("", "Erro ao cadastrar credenciais da API. Tente novamente.");
            return View(empresaAtual);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar credenciais da API");
            ModelState.AddModelError("", $"Erro ao cadastrar credenciais da API: {ex.Message}");
            var empresaAtual = await _apiService.GetAsync<EmpresaViewModel>("empresa/meus-dados");
            return View(empresaAtual ?? new EmpresaViewModel());
        }
    }

    // POST: Empresa/RemoverCredenciaisAPI
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverCredenciaisAPI()
    {
        // Verifica se está autenticado
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var success = await _apiService.DeleteAsync("empresa/credenciais-api");
            if (success)
            {
                TempData["SuccessMessage"] = "Credenciais da API removidas com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao remover credenciais da API. Tente novamente.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover credenciais da API");
            TempData["ErrorMessage"] = $"Erro ao remover credenciais da API: {ex.Message}";
        }

        return RedirectToAction("CredenciaisAPI");
    }
}

