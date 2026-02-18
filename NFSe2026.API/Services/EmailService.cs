using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace NFSe2026.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarEmailValidacaoAsync(string email, string nome, string codigo)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                fromEmail = smtpUser;
            }
            var fromName = _configuration["Email:FromName"] ?? "CAPRINDWEB";

            // Se não houver configuração de SMTP, apenas loga (para desenvolvimento)
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("Configuração de email não encontrada. Código de validação para {Email}: {Codigo}", email, codigo);
                return;
            }

            // Ignora validação do certificado SSL para servidores SMTP personalizados
            // ATENÇÃO: Use apenas se você confiar no servidor SMTP
            // Isso é necessário quando o certificado não corresponde ao nome do host
            ServicePointManager.ServerCertificateValidationCallback = 
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    // Aceita todos os certificados (incluindo auto-assinados)
                    // Em produção, considere validar o certificado adequadamente
                    return true;
                };

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "Código de Validação - CAPRINDWEB",
                Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='text-align:center; margin-bottom:20px;'>
                            <img src='https://caprindweb.com.br/img/logo-caprindweb.png'
                                 alt='CAPRINDWEB'
                                 style='max-width:140px; height:auto;' />
                        </div>
                        <h2 style='color: #667eea;'>Bem-vindo ao CAPRINDWEB!</h2>
                        <p>Olá <strong>{nome}</strong>,</p>
                        <p>Para confirmar seu cadastro e ativar sua conta, utilize o código de validação abaixo:</p>
                        <div style='background-color: #f8f9fa; border: 2px solid #667eea; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
                            <h1 style='color: #667eea; font-size: 32px; margin: 0; letter-spacing: 5px;'>{codigo}</h1>
                        </div>
                        <p>Este código expira em 24 horas.</p>
                        <p>Se você não solicitou este cadastro, ignore este email.</p>
                        <hr style='border: none; border-top: 1px solid #e9ecef; margin: 20px 0;' />
                        <p style='color: #6c757d; font-size: 12px;'>CAPRINDWEB - Sistema de Nota Fiscal de Serviços Eletrônica</p>
                    </body>
                    </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email de validação enviado para {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de validação para {Email}", email);
            // Não lança exceção para não bloquear o cadastro - apenas loga o código
            _logger.LogWarning("Código de validação para {Email}: {Codigo}", email, codigo);
        }
    }

    public async Task EnviarPDFPorEmailAsync(string email, string nomeTomador, string numeroNota, byte[] pdfBytes, string nomeArquivo)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                fromEmail = smtpUser;
            }
            var fromName = _configuration["Email:FromName"] ?? "CAPRINDWEB";

            // Se não houver configuração de SMTP, lança exceção
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("Configuração de email não encontrada. Não é possível enviar PDF para {Email}", email);
                throw new InvalidOperationException("Configuração de email não encontrada. Configure as credenciais SMTP no appsettings.json");
            }

            // Ignora validação do certificado SSL para servidores SMTP personalizados
            ServicePointManager.ServerCertificateValidationCallback = 
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"Nota Fiscal de Serviços Eletrônica - {numeroNota}",
                Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='text-align:center; margin-bottom:20px;'>
                            <img src='https://caprindweb.com.br/img/logo-caprindweb.png'
                                 alt='CAPRINDWEB'
                                 style='max-width:140px; height:auto;' />
                        </div>
                        <h2 style='color: #667eea;'>Nota Fiscal de Serviços Eletrônica</h2>
                        <p>Olá <strong>{nomeTomador}</strong>,</p>
                        <p>Segue em anexo a Nota Fiscal de Serviços Eletrônica número <strong>{numeroNota}</strong>.</p>
                        <p>Este é um email automático, por favor não responda.</p>
                        <hr style='border: none; border-top: 1px solid #e9ecef; margin: 20px 0;' />
                        <p style='color: #6c757d; font-size: 12px;'>CAPRINDWEB - Sistema de Nota Fiscal de Serviços Eletrônica</p>
                    </body>
                    </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            // Anexa o PDF
            using var memoryStream = new System.IO.MemoryStream(pdfBytes);
            var attachment = new Attachment(memoryStream, nomeArquivo, "application/pdf");
            mailMessage.Attachments.Add(attachment);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("PDF da nota fiscal {NumeroNota} enviado por email para {Email}", numeroNota, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar PDF da nota fiscal {NumeroNota} por email para {Email}", numeroNota, email);
            throw;
        }
    }
}

