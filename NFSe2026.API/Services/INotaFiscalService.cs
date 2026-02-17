using NFSe2026.API.DTOs;

namespace NFSe2026.API.Services;

public interface INotaFiscalService
{
    Task<NotaFiscalDTO> CriarNotaFiscalAsync(NotaFiscalCreateDTO notaFiscal, int empresaId);
    Task<NotaFiscalDTO> AtualizarNotaFiscalAsync(int id, NotaFiscalUpdateDTO notaFiscal, int empresaId);
    Task<NotaFiscalDTO> EmitirNotaFiscalAsync(int id, int empresaId);
    Task<NotaFiscalDTO?> ObterNotaFiscalPorIdAsync(int id, int empresaId);
    Task<IEnumerable<NotaFiscalDTO>> ListarNotasFiscaisAsync(int empresaId);
    Task<PagedResultDTO<NotaFiscalDTO>> ListarNotasFiscaisPaginadasAsync(int empresaId, int pageNumber, int pageSize);
    Task<bool> ExcluirNotaFiscalAsync(int id, int empresaId);
    Task<bool> CancelarNotaFiscalAsync(int id, string motivo, int empresaId);
    Task<NotaFiscalDTO?> ConsultarSituacaoAsync(int id, int empresaId);
    Task<NotaFiscalDTO?> ConsultarStatusAsync(int id, int empresaId);
    Task<string?> ObterXMLAsync(int id, int empresaId);
    Task<string?> ObterMotivoRejeicaoAsync(int id, int empresaId);
    Task<NotaFiscalDTO> ReverterParaRascunhoAsync(int id, int empresaId);
    Task<NotaFiscalDTO> CopiarNotaFiscalAsync(int id, int empresaId);
}

