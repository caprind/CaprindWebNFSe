using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NFSe2026.API.DTOs;
using NFSe2026.API.Services;

namespace NFSe2026.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost("cadastro")]
    public async Task<ActionResult<CadastroEmpresaResponseDTO>> CadastrarEmpresa([FromBody] CadastroEmpresaDTO cadastroDto)
    {
        try
        {
            var result = await _authService.CadastrarEmpresaAsync(cadastroDto);
            return CreatedAtAction(nameof(CadastrarEmpresa), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // ArgumentException geralmente indica erro de validação (ex: CNPJ inválido)
            _logger.LogWarning(ex, "Erro de validação ao cadastrar empresa: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar empresa");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPost("validar-email")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> ValidarEmail([FromBody] ValidarEmailDTO validarDto)
    {
        try
        {
            var result = await _authService.ValidarEmailAsync(validarDto.Email, validarDto.Codigo);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar email");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}

