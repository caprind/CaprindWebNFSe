using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace NFSe2026.API.Services;

public class CriptografiaService : ICriptografiaService
{
    private readonly string _chaveCriptografia;
    private readonly ILogger<CriptografiaService> _logger;

    public CriptografiaService(IConfiguration configuration, ILogger<CriptografiaService> logger)
    {
        // Usa a chave JWT como base para criptografia, mas pode ser uma chave específica
        _chaveCriptografia = configuration["Jwt:Key"] ?? "sua-chave-secreta-super-longa-e-complexa-para-producao-mude-isso-minimo-32-caracteres";
        _logger = logger;
    }

    public string Criptografar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;

        try
        {
            _logger.LogDebug("Criptografando texto. Tamanho: {Tamanho} caracteres, Contém caracteres especiais: {TemEspeciais}", 
                texto.Length, texto.Any(c => !char.IsLetterOrDigit(c)));
            
            // Gera um hash da chave para garantir 32 bytes (256 bits) para AES-256
            using var sha256 = SHA256.Create();
            var keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_chaveCriptografia));
            
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            // Garante encoding UTF-8 para suportar caracteres especiais como @, #, $, etc.
            var textoBytes = Encoding.UTF8.GetBytes(texto);
            var criptografado = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);

            // Combina IV + dados criptografados e converte para Base64
            var resultado = new byte[aes.IV.Length + criptografado.Length];
            Array.Copy(aes.IV, 0, resultado, 0, aes.IV.Length);
            Array.Copy(criptografado, 0, resultado, aes.IV.Length, criptografado.Length);

            var resultadoBase64 = Convert.ToBase64String(resultado);
            _logger.LogDebug("Texto criptografado com sucesso. Tamanho Base64: {Tamanho}", resultadoBase64.Length);
            return resultadoBase64;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criptografar texto. Tamanho do texto: {Tamanho}", texto?.Length ?? 0);
            throw;
        }
    }

    public string Descriptografar(string textoCriptografado)
    {
        if (string.IsNullOrEmpty(textoCriptografado))
            return string.Empty;

        try
        {
            _logger.LogDebug("Descriptografando texto. Tamanho Base64: {Tamanho}", textoCriptografado.Length);
            
            var dadosCompletos = Convert.FromBase64String(textoCriptografado);
            
            // Gera um hash da chave para garantir 32 bytes (256 bits) para AES-256
            using var sha256 = SHA256.Create();
            var keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_chaveCriptografia));

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC;

            // Extrai o IV (primeiros 16 bytes)
            var iv = new byte[16];
            Array.Copy(dadosCompletos, 0, iv, 0, 16);
            aes.IV = iv;

            // Extrai os dados criptografados (resto)
            var dadosCriptografados = new byte[dadosCompletos.Length - 16];
            Array.Copy(dadosCompletos, 16, dadosCriptografados, 0, dadosCriptografados.Length);

            using var decryptor = aes.CreateDecryptor();
            var descriptografado = decryptor.TransformFinalBlock(dadosCriptografados, 0, dadosCriptografados.Length);

            // Garante encoding UTF-8 para suportar caracteres especiais
            var resultado = Encoding.UTF8.GetString(descriptografado);
            _logger.LogDebug("Texto descriptografado com sucesso. Tamanho: {Tamanho} caracteres, Contém caracteres especiais: {TemEspeciais}", 
                resultado.Length, resultado.Any(c => !char.IsLetterOrDigit(c)));
            
            return resultado;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Erro ao descriptografar: formato Base64 inválido. Tamanho: {Tamanho}", textoCriptografado?.Length ?? 0);
            throw new Exception("Erro ao descriptografar: formato inválido. A senha pode ter sido corrompida.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao descriptografar texto. Tamanho Base64: {Tamanho}", textoCriptografado?.Length ?? 0);
            throw;
        }
    }
}



