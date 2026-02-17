using NFSe2026.API.DTOs;

namespace NFSe2026.API.Services;

/// <summary>
/// Interface abstrata para serviços de provedores de NFSe
/// </summary>
public interface IProvedorNFSeService
{
    /// <summary>
    /// Obtém o token de autenticação do provedor
    /// </summary>
    Task<string> ObterTokenAsync(int empresaId);
    
    /// <summary>
    /// Emite uma nota fiscal através do provedor
    /// </summary>
    /// <param name="notaFiscal">Dados da nota fiscal</param>
    /// <param name="empresaId">ID da empresa</param>
    /// <param name="notaFiscalId">ID da nota fiscal no banco de dados (opcional, usado para nDPS na NS Tecnologia)</param>
    /// <param name="numeroNota">Número da nota fiscal (opcional, usado para logs)</param>
    Task<EmitirNotaFiscalResult> EmitirNotaFiscalAsync(NotaFiscalCreateDTO notaFiscal, int empresaId, int? notaFiscalId = null, string? numeroNota = null);
    
    /// <summary>
    /// Cancela uma nota fiscal
    /// </summary>
    Task<bool> CancelarNotaFiscalAsync(string numero, string codigoVerificacao, string motivo, int empresaId);
    
    /// <summary>
    /// Consulta o status de uma nota fiscal
    /// </summary>
    Task<ConsultarNotaFiscalResult?> ConsultarNotaFiscalAsync(string numero, string codigoVerificacao, int empresaId);
    
    /// <summary>
    /// Retorna o nome do provedor
    /// </summary>
    string NomeProvedor { get; }
    
    /// <summary>
    /// Faz download do PDF da nota fiscal
    /// </summary>
    Task<byte[]?> DownloadPDFAsync(string chDPS, string chNFSe, string cnpj, int empresaId);
    
    /// <summary>
    /// Consulta o status de processamento da NFSe usando nsNRec (número do protocolo)
    /// </summary>
    /// <param name="nsNRec">Número do protocolo de recebimento</param>
    /// <param name="cnpj">CNPJ da empresa</param>
    /// <param name="empresaId">ID da empresa</param>
    /// <param name="notaFiscalId">ID da nota fiscal (opcional, usado para logs)</param>
    /// <param name="numeroNota">Número da nota fiscal (opcional, usado para logs)</param>
    Task<ConsultarNotaFiscalResult?> ConsultarStatusAsync(string nsNRec, string cnpj, int empresaId, int? notaFiscalId = null, string? numeroNota = null);
}

