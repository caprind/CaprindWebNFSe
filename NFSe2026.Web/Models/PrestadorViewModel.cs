using System.ComponentModel.DataAnnotations;

namespace NFSe2026.Web.Models;

public class PrestadorViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A razão social é obrigatória")]
    [Display(Name = "Razão Social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Display(Name = "Nome Fantasia")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "A inscrição municipal é obrigatória")]
    [Display(Name = "Inscrição Municipal")]
    public string InscricaoMunicipal { get; set; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório")]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

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

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Ambiente")]
    public int Ambiente { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; }
}

public class PrestadorCreateViewModel
{
    [Required(ErrorMessage = "A razão social é obrigatória")]
    [Display(Name = "Razão Social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Display(Name = "Nome Fantasia")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "A inscrição municipal é obrigatória")]
    [Display(Name = "Inscrição Municipal")]
    public string InscricaoMunicipal { get; set; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório")]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

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

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Ambiente")]
    public int Ambiente { get; set; } = 1; // Default: Homologacao

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;
}

