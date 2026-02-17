using System.Text;

namespace NFSe2026.API.Services;

/// <summary>
/// Logger específico para edição de tomadores
/// </summary>
public class TomadorEditLogger
{
    private readonly string _logDirectory;
    private readonly object _lockObject = new object();

    public TomadorEditLogger()
    {
        _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "tomador-edit");
        
        // Garante que o diretório existe
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    /// <summary>
    /// Escreve um log completo de edição de tomador
    /// </summary>
    public void LogEdicao(string conteudo)
    {
        try
        {
            var fileName = $"tomador-edit-{DateTime.Now:yyyyMMdd}.txt";
            var filePath = Path.Combine(_logDirectory, fileName);
            
            lock (_lockObject)
            {
                // Garante que o diretório ainda existe antes de escrever
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
                
                File.AppendAllText(filePath, conteudo + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
                Serilog.Log.Debug("Log de edição de tomador escrito em: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            // Se falhar, pelo menos tenta logar no log padrão
            Serilog.Log.Error(ex, "Erro ao escrever no log de edição de tomador. Diretório: {LogDirectory}", _logDirectory);
        }
    }

    /// <summary>
    /// Cria um formato completo de log para edição
    /// </summary>
    public string FormatLogCompleto(
        int tomadorId,
        string passo,
        string tipo,
        Dictionary<string, object> dados)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"Data/Hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"Tomador ID: {tomadorId}");
        sb.AppendLine($"Passo: {passo}");
        sb.AppendLine($"Tipo: {tipo}");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        
        if (dados != null && dados.Count > 0)
        {
            foreach (var item in dados)
            {
                var valor = item.Value?.ToString() ?? "null";
                // Ocultar valores sensíveis
                if (item.Key.Contains("senha", StringComparison.OrdinalIgnoreCase) ||
                    item.Key.Contains("password", StringComparison.OrdinalIgnoreCase))
                {
                    valor = "***";
                }
                sb.AppendLine($"{item.Key}: {valor}");
            }
        }
        
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        
        return sb.ToString();
    }
}

