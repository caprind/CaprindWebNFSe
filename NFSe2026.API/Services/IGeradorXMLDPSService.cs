using NFSe2026.API.DTOs;

namespace NFSe2026.API.Services;

public interface IGeradorXMLDPSService
{
    Task<string> GerarXMLDPSAsync(NotaFiscalCreateDTO notaFiscal, int empresaId);
}

