using System.Security.Cryptography.X509Certificates;

namespace NFSe2026.API.Services;

public interface IGeradorJWTService
{
    string GerarJWTAssinado(X509Certificate2 certificado, string cnpj, string ambiente);
}

