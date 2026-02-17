namespace NFSe2026.API.Configurations;

public class ApiNacionalNFSeSettings
{
    public const string SectionName = "ApiNacionalNFSe";

    public string UrlBase { get; set; } = string.Empty;
    public string? UrlBaseAuth { get; set; } // URL base específica para autenticação (pode ser diferente da UrlBase)
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
    public int Timeout { get; set; } = 30;
    public string Ambiente { get; set; } = "Homologacao";
    public string? EndpointToken { get; set; } = "/oauth/token"; // Endpoint para obter token OAuth
    public string? EndpointDPS { get; set; } = "https://sefin.producaorestrita.nfse.gov.br/API/SefinNacional/nfse"; // Endpoint para enviar DPS
}

