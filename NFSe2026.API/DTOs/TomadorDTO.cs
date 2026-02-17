namespace NFSe2026.API.DTOs;

public class TomadorDTO
{
    public int Id { get; set; }
    public int TipoPessoa { get; set; } // 1 = Fisica, 2 = Juridica
    public string CPFCNPJ { get; set; } = string.Empty;
    public string RazaoSocialNome { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
}

public class TomadorCreateDTO
{
    public int TipoPessoa { get; set; } // 1 = Fisica, 2 = Juridica
    public string CPFCNPJ { get; set; } = string.Empty;
    public string RazaoSocialNome { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
}

public class TomadorUpdateDTO
{
    public int TipoPessoa { get; set; } // 1 = Fisica, 2 = Juridica
    public string RazaoSocialNome { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
}

public class TomadorPorCNPJDTO
{
    public string CNPJ { get; set; } = string.Empty;
}

