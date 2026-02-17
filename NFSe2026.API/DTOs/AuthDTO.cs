namespace NFSe2026.API.DTOs;

public class LoginDTO
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public UsuarioDTO Usuario { get; set; } = null!;
    public EmpresaDTO Empresa { get; set; } = null!;
    public DateTime ExpiraEm { get; set; }
}

public class CadastroEmpresaDTO
{
    public string CNPJ { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Logotipo { get; set; } // Base64 da imagem do logotipo
}

public class CadastroEmpresaResponseDTO
{
    public EmpresaDTO Empresa { get; set; } = null!;
    public UsuarioDTO Usuario { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public bool EmailEnviado { get; set; }
}

public class ValidarEmailDTO
{
    public string Email { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

