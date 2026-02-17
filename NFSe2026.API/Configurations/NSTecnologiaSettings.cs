namespace NFSe2026.API.Configurations;

/// <summary>
/// Configurações para a API da NS Tecnologia
/// </summary>
public class NSTecnologiaSettings
{
    public const string SectionName = "NSTecnologia";
    
    /// <summary>
    /// URL base da API da NS Tecnologia
    /// </summary>
    public string UrlBase { get; set; } = string.Empty;
    
    /// <summary>
    /// Token de autenticação da NS Tecnologia (será substituído por credenciais da empresa)
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// CNPJ do emitente (pode ser configurado globalmente ou por empresa)
    /// </summary>
    public string? CNPJ { get; set; }
    
    /// <summary>
    /// Timeout em segundos para requisições HTTP
    /// </summary>
    public int Timeout { get; set; } = 60;
    
    /// <summary>
    /// Ambiente (Homologacao ou Producao)
    /// </summary>
    public string Ambiente { get; set; } = "Homologacao";
    
    /// <summary>
    /// Endpoint para emissão de NFSe (Homologação e Produção usam o mesmo endpoint)
    /// </summary>
    public string? EndpointEmitir { get; set; } = "/nfse/issue";
    
    /// <summary>
    /// Endpoint para emissão de NFSe (Produção) - mesmo endpoint da homologação
    /// </summary>
    public string? EndpointEmitirProducao { get; set; } = "/nfse/issue";
    
    /// <summary>
    /// Endpoint para cancelamento de NFSe
    /// </summary>
    public string? EndpointCancelar { get; set; } = "/api/nfse/cancelar";
    
    /// <summary>
    /// Endpoint para consulta de NFSe
    /// </summary>
    public string? EndpointConsultar { get; set; } = "/api/nfse/consultar";
    
    /// <summary>
    /// Endpoint para consulta de status de processamento
    /// </summary>
    public string? EndpointStatus { get; set; } = "/nfse/issue/status";
}

