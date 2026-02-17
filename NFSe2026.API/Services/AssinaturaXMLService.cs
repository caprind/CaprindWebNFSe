using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace NFSe2026.API.Services;

public class AssinaturaXMLService : IAssinaturaXMLService
{
    private readonly ILogger<AssinaturaXMLService> _logger;

    public AssinaturaXMLService(ILogger<AssinaturaXMLService> logger)
    {
        _logger = logger;
    }

    public X509Certificate2 CarregarCertificado(string certificadoBase64, string senha)
    {
        try
        {
            // Validações iniciais
            if (string.IsNullOrWhiteSpace(certificadoBase64))
            {
                throw new ArgumentException("Certificado digital não pode ser vazio ou nulo.");
            }

            if (string.IsNullOrWhiteSpace(senha))
            {
                throw new ArgumentException("Senha do certificado não pode ser vazia ou nula.");
            }

            // Log da senha (apenas tamanho e se tem caracteres especiais, nunca o valor real)
            _logger.LogInformation("Validando senha do certificado. Tamanho: {Tamanho} caracteres, Contém caracteres especiais: {TemEspeciais}", 
                senha.Length, senha.Any(c => !char.IsLetterOrDigit(c)));

            // Valida se é Base64 válido e não foi truncado
            if (certificadoBase64.Length < 100)
            {
                _logger.LogWarning("Certificado Base64 parece muito curto ({Tamanho} caracteres). Pode estar incompleto.", certificadoBase64.Length);
                throw new ArgumentException("Certificado digital muito pequeno. Pode estar incompleto ou corrompido.");
            }
            
            // Verifica se o certificado pode ter sido truncado (se tiver exatamente 5000 caracteres, pode ter sido limitado pelo banco antigo)
            if (certificadoBase64.Length == 5000)
            {
                _logger.LogError("Certificado tem exatamente 5000 caracteres - pode ter sido truncado pelo limite do banco de dados. Recadastre o certificado após atualizar o banco.");
                throw new Exception("Certificado digital pode estar truncado (exatamente 5000 caracteres). Por favor, recadastre o certificado após atualizar o banco de dados para suportar certificados maiores.");
            }

            _logger.LogInformation("Tentando converter Base64 para bytes. Tamanho Base64: {Tamanho} caracteres", certificadoBase64.Length);
            
            byte[] certificadoBytes;
            try
            {
                certificadoBytes = Convert.FromBase64String(certificadoBase64);
                _logger.LogInformation("Base64 convertido com sucesso. Tamanho em bytes: {TamanhoBytes}", certificadoBytes.Length);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Erro ao converter Base64. O certificado pode estar corrompido ou em formato inválido.");
                throw new Exception("O certificado digital está em formato Base64 inválido. Verifique se o arquivo foi carregado corretamente.", ex);
            }

            // Tenta carregar o certificado com diferentes flags
            X509Certificate2? certificado = null;
            var flagsToTry = new[]
            {
                // Primeira tentativa: flags padrão para aplicações web
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable,
                // Segunda tentativa: flags mais permissivos
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable,
                // Terceira tentativa: flags mínimos
                X509KeyStorageFlags.Exportable,
                // Quarta tentativa: sem flags especiais
                X509KeyStorageFlags.DefaultKeySet
            };

            Exception? lastException = null;
            foreach (var flags in flagsToTry)
            {
                try
                {
                    _logger.LogInformation("Tentando carregar certificado com flags: {Flags}", flags);
                    certificado = new X509Certificate2(certificadoBytes, senha, flags);
                    
                    // Valida se o certificado foi carregado corretamente
                    if (certificado != null && certificado.HasPrivateKey)
                    {
                        _logger.LogInformation("Certificado digital carregado com sucesso usando flags {Flags}. Válido até: {DataVencimento}, Subject: {Subject}", 
                            flags, certificado.NotAfter, certificado.Subject);
                        return certificado;
                    }
                    else if (certificado != null)
                    {
                        _logger.LogWarning("Certificado carregado mas não possui chave privada. Tentando próximo conjunto de flags.");
                        certificado.Dispose();
                        certificado = null;
                    }
                }
                catch (CryptographicException ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Falha ao carregar certificado com flags {Flags}. Tentando próximo conjunto.", flags);
                    certificado?.Dispose();
                    certificado = null;
                }
            }

            // Se chegou aqui, nenhum conjunto de flags funcionou
            if (lastException != null)
            {
                var errorMessage = lastException.Message.ToLower();
                if (errorMessage.Contains("senha") || errorMessage.Contains("password") || errorMessage.Contains("incorrect"))
                {
                    _logger.LogError(lastException, "Erro ao carregar certificado: senha incorreta ou certificado protegido. Mensagem original: {Mensagem}", lastException.Message);
                    throw new Exception("❌ Senha do certificado digital incorreta. Verifique se a senha informada está correta, incluindo caracteres especiais como @, #, $, etc. Certifique-se de que não há espaços extras no início ou fim da senha.", lastException);
                }
                else if (errorMessage.Contains("objeto necessário não foi encontrado") || errorMessage.Contains("object not found"))
                {
                    _logger.LogError(lastException, "Erro ao carregar certificado: certificado pode estar corrompido ou não é um arquivo .pfx/.p12 válido.");
                    throw new Exception("O certificado digital não pôde ser carregado. Verifique se: 1) O arquivo é um certificado .pfx ou .p12 válido; 2) O certificado não está corrompido; 3) A senha está correta.", lastException);
                }
                else
                {
                    _logger.LogError(lastException, "Erro ao carregar certificado digital com todos os conjuntos de flags testados.");
                    throw new Exception($"Erro ao carregar certificado digital: {lastException.Message}. Verifique se o certificado e a senha estão corretos.", lastException);
                }
            }

            throw new Exception("Não foi possível carregar o certificado digital com nenhum conjunto de flags testado.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Erro de validação ao carregar certificado digital");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao carregar certificado digital");
            throw new Exception("Erro ao carregar certificado digital. Verifique se o certificado e a senha estão corretos.", ex);
        }
    }

    public string AssinarXML(string xml, X509Certificate2 certificado)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xml);

            // Verifica se o certificado tem chave privada
            if (!certificado.HasPrivateKey)
            {
                throw new Exception("O certificado não possui chave privada. Certificado A1 deve ter a chave privada.");
            }

            // Cria a assinatura XML
            var signedXml = new SignedXml(xmlDoc);
            
            // Obtém a chave privada (RSA ou ECDsa)
            AsymmetricAlgorithm? privateKey = null;
            try
            {
                privateKey = certificado.GetRSAPrivateKey();
            }
            catch
            {
                try
                {
                    privateKey = certificado.GetECDsaPrivateKey();
                }
                catch
                {
                    throw new Exception("Não foi possível obter a chave privada do certificado. Certifique-se de que é um certificado A1 válido.");
                }
            }

            if (privateKey == null)
            {
                throw new Exception("Chave privada não encontrada no certificado.");
            }

            signedXml.SigningKey = privateKey;

            // Encontra o elemento raiz
            var rootElement = xmlDoc.DocumentElement;
            if (rootElement == null)
            {
                throw new Exception("XML não possui elemento raiz.");
            }

            // Verifica se o elemento raiz já tem um ID (case-sensitive: "Id" ou "id")
            string? rootId = null;
            if (rootElement.HasAttribute("Id"))
            {
                rootId = rootElement.GetAttribute("Id");
                _logger.LogInformation("ID encontrado no elemento raiz: {Id}", rootId);
            }
            else if (rootElement.HasAttribute("id"))
            {
                rootId = rootElement.GetAttribute("id");
                _logger.LogInformation("ID encontrado no elemento raiz (lowercase): {Id}", rootId);
            }

            // Se não tiver ID, cria um ID baseado no nome do elemento
            if (string.IsNullOrEmpty(rootId))
            {
                rootId = rootElement.LocalName + "_" + Guid.NewGuid().ToString("N").Substring(0, 16);
                rootElement.SetAttribute("Id", rootId);
                _logger.LogInformation("ID gerado e adicionado ao elemento raiz: {Id}", rootId);
            }

            // Referência ao documento usando o ID (URI deve começar com #)
            var reference = new Reference { Uri = "#" + rootId };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());
            signedXml.AddReference(reference);
            
            _logger.LogInformation("Assinatura configurada para referenciar ID: {Id}", rootId);

            // Informações do certificado
            var keyInfo = new KeyInfo();
            var keyInfoData = new KeyInfoX509Data(certificado);
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            // Computa a assinatura
            signedXml.ComputeSignature();

            // Adiciona a assinatura ao XML
            var xmlElement = signedXml.GetXml();
            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlElement, true));

            _logger.LogInformation("XML assinado com sucesso usando certificado {Subject}", certificado.Subject);
            return xmlDoc.OuterXml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao assinar XML");
            throw new Exception("Erro ao assinar XML com certificado digital.", ex);
        }
    }
}

