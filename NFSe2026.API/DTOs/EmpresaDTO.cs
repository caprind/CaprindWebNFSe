namespace NFSe2026.API.DTOs;

public class EmpresaDTO
{
    public int Id { get; set; }
    public string CNPJ { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string? CodigoMunicipio { get; set; } // Código IBGE do município
    public string? RegimeEspecialTributacao { get; set; } // Regime especial de tributação
    public bool OptanteSimplesNacional { get; set; } = true; // Indica se a empresa é optante do Simples Nacional
    public bool IncentivoFiscal { get; set; } = false; // Indica se há incentivo fiscal
    public string CEP { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? SituacaoCadastral { get; set; }
    public string? Logotipo { get; set; } // Base64 da imagem do logotipo
    public DateTime? DataVencimentoCertificado { get; set; } // Data de vencimento do certificado digital
    public bool TemCertificadoDigital { get; set; } // Indica se tem certificado cadastrado
    public bool TemClientIdSecret { get; set; } // Indica se tem ClientId e ClientSecret cadastrados
    public int ProvedorNFSe { get; set; } // Provedor de NFSe (1=Nacional, 2=NS Tecnologia)
    public bool Ativo { get; set; }
}

public class EmpresaUpdateDTO
{
    public string? NomeFantasia { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? UF { get; set; }
    public string? CodigoMunicipio { get; set; }
    public string? CEP { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Logotipo { get; set; } // Base64 da imagem do logotipo
    public string? RegimeEspecialTributacao { get; set; }
    public bool? OptanteSimplesNacional { get; set; }
    public bool? IncentivoFiscal { get; set; }
    public int? ProvedorNFSe { get; set; } // Provedor de NFSe (1=Nacional, 2=NS Tecnologia)
    // Nota: Certificado digital deve ser gerenciado pelo endpoint específico /certificado-digital
    // Nota: ClientId e ClientSecret devem ser gerenciados pelo endpoint específico /credenciais-api
}

public class ConsultarCNPJDTO
{
    public string CNPJ { get; set; } = string.Empty;
}

public class CertificadoDigitalDTO
{
    public string CertificadoDigital { get; set; } = string.Empty; // Base64 do arquivo .pfx ou .p12
    public string SenhaCertificado { get; set; } = string.Empty; // Senha do certificado (será criptografada)
}

public class CredenciaisAPIDTO
{
    public string ClientId { get; set; } = string.Empty; // ClientId/Token da API (Nacional NFSe ou NS Tecnologia) - será criptografado
    public string? ClientSecret { get; set; } // ClientSecret da API (opcional - necessário apenas para API Nacional NFSe) - será criptografado
}

