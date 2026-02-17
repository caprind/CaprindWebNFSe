using NFSe2026.API.DTOs;

namespace NFSe2026.API.Services;

/// <summary>
/// Interface para o serviço da API Nacional NFSe (mantida para compatibilidade)
/// </summary>
public interface INFSeAPIService : IProvedorNFSeService
{
    // Esta interface agora herda de IProvedorNFSeService para manter compatibilidade
    // O NomeProvedor deve retornar "API Nacional NFSe"
}

