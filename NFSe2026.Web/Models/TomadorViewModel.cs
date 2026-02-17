using System.ComponentModel.DataAnnotations;

namespace NFSe2026.Web.Models;

public class TomadorViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O tipo de pessoa é obrigatório")]
    [Display(Name = "Tipo de Pessoa")]
    public int TipoPessoa { get; set; }

    [Required(ErrorMessage = "O CPF/CNPJ é obrigatório")]
    [Display(Name = "CPF/CNPJ")]
    public string CPFCNPJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome/razão social é obrigatório")]
    [Display(Name = "Nome/Razão Social")]
    public string RazaoSocialNome { get; set; } = string.Empty;

    [Display(Name = "Inscrição Estadual")]
    public string? InscricaoEstadual { get; set; }

    [Display(Name = "Inscrição Municipal")]
    public string? InscricaoMunicipal { get; set; }

    [Required(ErrorMessage = "O endereço é obrigatório")]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "O número é obrigatório")]
    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;

    [Display(Name = "Complemento")]
    public string? Complemento { get; set; }

    [Required(ErrorMessage = "O bairro é obrigatório")]
    [Display(Name = "Bairro")]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória")]
    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "A UF é obrigatória")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve ter 2 caracteres")]
    [Display(Name = "UF")]
    public string UF { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "O CEP deve ter 8 dígitos")]
    [Display(Name = "CEP")]
    public string CEP { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }
}

public class TomadorCreateViewModel
{
    [Required(ErrorMessage = "O tipo de pessoa é obrigatório")]
    [Display(Name = "Tipo de Pessoa")]
    public int TipoPessoa { get; set; }

    [Required(ErrorMessage = "O CPF/CNPJ é obrigatório")]
    [Display(Name = "CPF/CNPJ")]
    public string CPFCNPJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome/razão social é obrigatório")]
    [Display(Name = "Nome/Razão Social")]
    public string RazaoSocialNome { get; set; } = string.Empty;

    [Display(Name = "Inscrição Estadual")]
    public string? InscricaoEstadual { get; set; }

    [Display(Name = "Inscrição Municipal")]
    public string? InscricaoMunicipal { get; set; }

    [Required(ErrorMessage = "O endereço é obrigatório")]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "O número é obrigatório")]
    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;

    [Display(Name = "Complemento")]
    public string? Complemento { get; set; }

    [Required(ErrorMessage = "O bairro é obrigatório")]
    [Display(Name = "Bairro")]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória")]
    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "A UF é obrigatória")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve ter 2 caracteres")]
    [Display(Name = "UF")]
    public string UF { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "O CEP deve ter 8 dígitos")]
    [Display(Name = "CEP")]
    public string CEP { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }
}

public class TomadorPorCNPJViewModel
{
    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;
}

