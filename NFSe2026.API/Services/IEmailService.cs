namespace NFSe2026.API.Services;

public interface IEmailService
{
    Task EnviarEmailValidacaoAsync(string email, string nome, string codigo);
    Task EnviarPDFPorEmailAsync(string email, string nomeTomador, string numeroNota, byte[] pdfBytes, string nomeArquivo);
}

