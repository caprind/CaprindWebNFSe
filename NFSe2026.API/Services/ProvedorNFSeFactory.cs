using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NFSe2026.API.Data;
using NFSe2026.API.Models;

namespace NFSe2026.API.Services;

/// <summary>
/// Factory para obter o serviço do provedor de NFSe correto baseado na configuração da empresa
/// </summary>
public class ProvedorNFSeFactory : IProvedorNFSeFactory
{
    private readonly ApplicationDbContext _context;
    private readonly INFSeAPIService _nacionalService;
    private readonly NSTecnologiaAPIService _nsTecnologiaService;
    private readonly ILogger<ProvedorNFSeFactory> _logger;

    public ProvedorNFSeFactory(
        ApplicationDbContext context,
        INFSeAPIService nacionalService,
        NSTecnologiaAPIService nsTecnologiaService,
        ILogger<ProvedorNFSeFactory> logger)
    {
        _context = context;
        _nacionalService = nacionalService;
        _nsTecnologiaService = nsTecnologiaService;
        _logger = logger;
    }

    public async Task<IProvedorNFSeService> ObterProvedorAsync(int empresaId)
    {
        try
        {
            // Tenta obter o ProvedorNFSe usando SQL raw para evitar erro se a coluna não existir
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ProvedorNFSe FROM Empresas WHERE Id = @empresaId LIMIT 1";
            var param = command.CreateParameter();
            param.ParameterName = "@empresaId";
            param.Value = empresaId;
            command.Parameters.Add(param);
            
            var result = await command.ExecuteScalarAsync();
            await connection.CloseAsync();
            
            if (result != null && result != DBNull.Value)
            {
                var provedorValue = Convert.ToInt32(result);
                _logger.LogInformation("ProvedorNFSe encontrado para empresa {EmpresaId}: {Provedor}", empresaId, provedorValue);
                return provedorValue switch
                {
                    1 => _nacionalService,      // Nacional
                    2 => _nsTecnologiaService,  // NS Tecnologia
                    _ => _nacionalService // Fallback para Nacional
                };
            }
            else
            {
                _logger.LogInformation("ProvedorNFSe não encontrado ou NULL para empresa {EmpresaId}, usando padrão (Nacional)", empresaId);
            }
        }
        catch (Exception ex)
        {
            // Se houver erro (coluna não existe ou outro problema), usa o padrão
            _logger.LogWarning(ex, "Não foi possível obter ProvedorNFSe da empresa {EmpresaId}, usando padrão (Nacional). Erro: {Mensagem}", 
                empresaId, ex.Message);
        }

        // Default para Nacional se não conseguir obter ou se houver erro
        _logger.LogInformation("Usando provedor padrão (Nacional) para empresa {EmpresaId}", empresaId);
        return _nacionalService;
    }
}

/// <summary>
/// Interface para o factory de provedores
/// </summary>
public interface IProvedorNFSeFactory
{
    Task<IProvedorNFSeService> ObterProvedorAsync(int empresaId);
}

