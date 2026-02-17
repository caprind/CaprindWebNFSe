using NFSe2026.API.DTOs;

namespace NFSe2026.API.Services;

public interface IAuthService
{
    Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto);
    Task<CadastroEmpresaResponseDTO> CadastrarEmpresaAsync(CadastroEmpresaDTO cadastroDto);
    Task<LoginResponseDTO> ValidarEmailAsync(string email, string codigo);
    string GenerateJwtToken(int usuarioId, int empresaId, string email);
}
