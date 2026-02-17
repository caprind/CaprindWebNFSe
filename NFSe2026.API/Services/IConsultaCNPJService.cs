namespace NFSe2026.API.Services;

public interface IConsultaCNPJService
{
    Task<ConsultaCNPJResult?> ConsultarCNPJAsync(string cnpj);
}

public class ConsultaCNPJResult
{
    public string CNPJ { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? SituacaoCadastral { get; set; }
    public string? Porte { get; set; }
    public string? NaturezaJuridica { get; set; }
    public DateTime? DataAbertura { get; set; }
    public EnderecoCNPJ? Endereco { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}

public class EnderecoCNPJ
{
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
}

