using NFSe2026.API.Models;

namespace NFSe2026.API.Services;

public class EmitirNotaFiscalResult
{
    public string? Numero { get; set; }
    public string? CodigoVerificacao { get; set; }
    public int Situacao { get; set; } // Usa int para compatibilidade
    public string? XML { get; set; }
    public string? JSON { get; set; }
    public string? PDFUrl { get; set; } // URL do PDF da nota fiscal (quando disponível)
    public byte[]? PDF { get; set; } // Dados binários do PDF (base64 decodificado)
    public string? NsNRec { get; set; } // Número do protocolo de recebimento (usado para consultar status)
}

public class ConsultarNotaFiscalResult
{
    public string? Numero { get; set; }
    public string? CodigoVerificacao { get; set; }
    public int Situacao { get; set; } // Usa int para compatibilidade
    public string? XML { get; set; }
    public string? JSON { get; set; }
    public string? ChDPS { get; set; } // Chave de acesso da DPS
    public string? ChNFSe { get; set; } // Chave de acesso da NFSe
    public string? NsNRec { get; set; } // Número do protocolo de recebimento
    public string? XMotivo { get; set; } // Motivo/descrição do status
}

