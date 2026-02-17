using System.ComponentModel.DataAnnotations;

namespace NFSe2026.Web.Models;

public class EmpresaViewModel
{
    public int Id { get; set; }

    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;

    [Display(Name = "Razão Social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Display(Name = "Nome Fantasia")]
    public string? NomeFantasia { get; set; }

    [Display(Name = "Inscrição Estadual")]
    public string? InscricaoEstadual { get; set; }

    [Display(Name = "Inscrição Municipal")]
    public string? InscricaoMunicipal { get; set; }

    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;

    [Display(Name = "Complemento")]
    public string? Complemento { get; set; }

    [Display(Name = "Bairro")]
    public string Bairro { get; set; } = string.Empty;

    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = string.Empty;

    [Display(Name = "UF")]
    public string UF { get; set; } = string.Empty;

    [Display(Name = "Código do Município (IBGE)")]
    public string? CodigoMunicipio { get; set; }

    [Display(Name = "CEP")]
    public string CEP { get; set; } = string.Empty;

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Display(Name = "Email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Logotipo")]
    public string? Logotipo { get; set; } // Base64 da imagem

    [Display(Name = "Situação Cadastral")]
    public string? SituacaoCadastral { get; set; }
    
    [Display(Name = "Data de Vencimento do Certificado")]
    [DataType(DataType.Date)]
    public DateTime? DataVencimentoCertificado { get; set; }
    
    [Display(Name = "Tem Certificado Digital")]
    public bool TemCertificadoDigital { get; set; }
    
    [Display(Name = "Tem Credenciais da API")]
    public bool TemClientIdSecret { get; set; }
    
    [Display(Name = "Provedor de NFSe")]
    public int ProvedorNFSe { get; set; } = 1; // 1 = Nacional, 2 = NS Tecnologia

    public bool Ativo { get; set; }
}

public class EmpresaEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "CNPJ")]
    public string CNPJ { get; set; } = string.Empty;

    [Display(Name = "Razão Social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Display(Name = "Nome Fantasia")]
    public string? NomeFantasia { get; set; }

    [Display(Name = "Inscrição Estadual")]
    public string? InscricaoEstadual { get; set; }

    [Display(Name = "Inscrição Municipal")]
    public string? InscricaoMunicipal { get; set; }

    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }

    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [Display(Name = "Complemento")]
    public string? Complemento { get; set; }

    [Display(Name = "Bairro")]
    public string? Bairro { get; set; }

    [Display(Name = "Cidade")]
    public string? Cidade { get; set; }

    [Display(Name = "UF")]
    [StringLength(2)]
    public string? UF { get; set; }

    [Display(Name = "Código do Município (IBGE)")]
    [StringLength(7)]
    public string? CodigoMunicipio { get; set; }

    [Display(Name = "CEP")]
    [StringLength(8)]
    public string? CEP { get; set; }

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Display(Name = "Email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Logotipo")]
    public string? Logotipo { get; set; } // Base64 da imagem

    [Display(Name = "Logotipo Atual")]
    public string? LogotipoAtual { get; set; } // Para exibir a imagem atual
    
    [Display(Name = "Data de Vencimento do Certificado")]
    [DataType(DataType.Date)]
    public DateTime? DataVencimentoCertificado { get; set; }
    
    [Display(Name = "Tem Certificado Digital")]
    public bool TemCertificadoDigital { get; set; }
    
    [Display(Name = "Tem Credenciais da API")]
    public bool TemClientIdSecret { get; set; }
    
    [Display(Name = "Provedor de NFSe")]
    public int ProvedorNFSe { get; set; } = 1; // 1 = Nacional, 2 = NS Tecnologia
}

