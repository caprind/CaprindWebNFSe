using System.Security.Cryptography.X509Certificates;

namespace NFSe2026.API.Services;

public interface IAssinaturaXMLService
{
    string AssinarXML(string xml, X509Certificate2 certificado);
    X509Certificate2 CarregarCertificado(string certificadoBase64, string senha);
}

