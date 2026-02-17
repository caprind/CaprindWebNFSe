using System.ComponentModel.DataAnnotations;

namespace NFSe2026.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;
}

public class CadastroViewModel
{
    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome é obrigatório")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Senha")]
    [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Display(Name = "Logotipo da Empresa")]
    [DataType(DataType.Upload)]
    public IFormFile? LogotipoFile { get; set; }

    public string? Logotipo { get; set; } // Base64 da imagem
}

public class LoginResponseModel
{
    public string Token { get; set; } = string.Empty;
    public UsuarioModel Usuario { get; set; } = null!;
    public EmpresaModel Empresa { get; set; } = null!;
    public DateTime ExpiraEm { get; set; }
}

public class UsuarioModel
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
}

public class EmpresaModel
{
    public int Id { get; set; }
    public string CNPJ { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
}

public class ValidarEmailViewModel
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O código de validação é obrigatório")]
    [Display(Name = "Código de Validação")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "O código deve ter 6 dígitos")]
    public string Codigo { get; set; } = string.Empty;
}

