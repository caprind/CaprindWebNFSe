using NFSe2026.API.Models;

namespace NFSe2026.API.DTOs;

public class PrestadorDTO
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string CNPJ { get; set; } = string.Empty;
    public string InscricaoMunicipal { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public Ambiente Ambiente { get; set; }
    public bool Ativo { get; set; }
}

public class PrestadorCreateDTO
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string CNPJ { get; set; } = string.Empty;
    public string InscricaoMunicipal { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? CertificadoDigital { get; set; }
    public string? SenhaCertificado { get; set; }
    public Ambiente Ambiente { get; set; } = Ambiente.Homologacao;
}

public class PrestadorUpdateDTO
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string InscricaoMunicipal { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? CertificadoDigital { get; set; }
    public string? SenhaCertificado { get; set; }
    public Ambiente Ambiente { get; set; }
    public bool Ativo { get; set; }
}

