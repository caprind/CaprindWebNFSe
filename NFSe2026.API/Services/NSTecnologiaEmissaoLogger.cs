using System.IO;
using System.Text;

namespace NFSe2026.API.Services;

/// <summary>
/// Logger específico para emissões de NFSe via NS Tecnologia
/// Cria um arquivo de log por ID da nota fiscal (todos os passos no mesmo arquivo)
/// </summary>
public class NSTecnologiaEmissaoLogger
{
    private readonly string _logDirectory;
    private readonly Dictionary<string, object> _lockObjects = new Dictionary<string, object>();
    private readonly object _lockObjectsLock = new object();

    public NSTecnologiaEmissaoLogger()
    {
        _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "emissao-ns-tecnologia");
        
        // Garante que o diretório existe
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    /// <summary>
    /// Obtém ou cria um lock object para um identificador específico
    /// </summary>
    private object GetLockObject(string identificador)
    {
        lock (_lockObjectsLock)
        {
            if (!_lockObjects.ContainsKey(identificador))
            {
                _lockObjects[identificador] = new object();
            }
            return _lockObjects[identificador];
        }
    }

    /// <summary>
    /// Escreve um log completo de emissão NS Tecnologia
    /// </summary>
    /// <param name="identificador">Identificador da nota (nsNRec, número da nota, ou ID temporário)</param>
    /// <param name="conteudo">Conteúdo do log</param>
    public void LogEmissao(string identificador, string conteudo)
    {
        try
        {
            // Remove caracteres inválidos para nome de arquivo
            var identificadorLimpo = SanitizeFileName(identificador);
            // Nome do arquivo agora usa apenas o número da nota (sem prefixo)
            var fileName = $"{identificadorLimpo}.txt";
            var filePath = Path.Combine(_logDirectory, fileName);
            
            var lockObject = GetLockObject(identificador);
            lock (lockObject)
            {
                File.AppendAllText(filePath, conteudo + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            // Se falhar, pelo menos tenta logar no log padrão
            Serilog.Log.Error(ex, "Erro ao escrever no log de emissão NS Tecnologia para identificador: {Identificador}", identificador);
        }
    }

    /// <summary>
    /// Remove caracteres inválidos de um nome de arquivo
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;
        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }
        return sanitized;
    }

    /// <summary>
    /// Cria um formato completo de log para emissão
    /// </summary>
    public string FormatLogCompleto(
        int notaFiscalId,
        string passo,
        string tipo,
        Dictionary<string, object> dados)
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"Data/Hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"Nota Fiscal ID: {notaFiscalId}");
        sb.AppendLine($"Passo: {passo}");
        sb.AppendLine($"Tipo: {tipo}");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        
        foreach (var item in dados)
        {
            sb.AppendLine($"{item.Key}: {item.Value}");
        }
        
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        
        return sb.ToString();
    }

    /// <summary>
    /// Determina o identificador a ser usado no nome do arquivo
    /// Usa o número da nota fiscal se disponível, caso contrário usa o ID
    /// </summary>
    public static string ObterIdentificadorLog(string? nsNRec, string? numero, int notaFiscalId)
    {
        // Usa o número da nota fiscal se disponível, caso contrário usa o ID
        if (!string.IsNullOrWhiteSpace(numero))
        {
            // Remove formatação e espaços
            var numeroLimpo = numero.Trim().Replace(" ", "");
            return numeroLimpo;
        }
        return $"ID-{notaFiscalId}";
    }
}

