using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace NFSe2026.API.Services;

public class GeradorJWTService : IGeradorJWTService
{
    private readonly ILogger<GeradorJWTService> _logger;

    public GeradorJWTService(ILogger<GeradorJWTService> logger)
    {
        _logger = logger;
    }

    public string GerarJWTAssinado(X509Certificate2 certificado, string cnpj, string ambiente)
    {
        try
        {
            // Verifica se o certificado tem chave privada
            if (!certificado.HasPrivateKey)
            {
                throw new Exception("O certificado não possui chave privada. Certificado A1 ou A3 deve ter a chave privada.");
            }

            // Obtém a chave RSA do certificado
            RSA? rsaPrivateKey = null;
            try
            {
                rsaPrivateKey = certificado.GetRSAPrivateKey();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter chave RSA do certificado");
                throw new Exception("Erro ao obter chave privada RSA do certificado. Certifique-se de que é um certificado A1 ou A3 válido.", ex);
            }

            if (rsaPrivateKey == null)
            {
                throw new Exception("Não foi possível obter a chave privada RSA do certificado.");
            }

            // Cria a chave de assinatura usando a chave privada do certificado
            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsaPrivateKey),
                SecurityAlgorithms.RsaSha256) // RS256
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            // Determina o issuer e audience baseado no ambiente
            // Ajustar conforme documentação da API Nacional
            var issuer = ambiente.ToLower() == "producao" 
                ? "https://www.nfse.gov.br" 
                : "https://www.producaorestrita.nfse.gov.br";
            
            var audience = ambiente.ToLower() == "producao"
                ? "https://www.nfse.gov.br/oauth/token"
                : "https://www.producaorestrita.nfse.gov.br/oauth/token";

            // Claims conforme especificação da RFB
            var claims = new List<Claim>
            {
                // Issuer: CNPJ do prestador
                new Claim(JwtRegisteredClaimNames.Iss, cnpj),
                
                // Subject: CNPJ do prestador
                new Claim(JwtRegisteredClaimNames.Sub, cnpj),
                
                // Audience: endpoint de token
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                
                // JWT ID único
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                
                // Issued at: momento da emissão
                new Claim(JwtRegisteredClaimNames.Iat, 
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64),
                
                // Expiration: 5 minutos (conforme especificação)
                new Claim(JwtRegisteredClaimNames.Exp, 
                    DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64),
                
                // CNPJ do prestador (claim customizado)
                new Claim("cnpj", cnpj),
                
                // Ambiente
                new Claim("ambiente", ambiente.ToLower())
            };

            // Cria o token JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5), // Token expira em 5 minutos
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            _logger.LogInformation("JWT assinado gerado com sucesso para CNPJ {CNPJ} no ambiente {Ambiente}", cnpj, ambiente);
            
            return jwtToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar JWT assinado com certificado digital");
            throw new Exception("Erro ao gerar JWT assinado com certificado digital.", ex);
        }
    }
}

