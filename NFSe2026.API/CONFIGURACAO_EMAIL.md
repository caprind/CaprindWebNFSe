# Configuração de Email

Para que o sistema envie emails de validação, é necessário configurar as credenciais SMTP no arquivo `appsettings.json`.

## Configuração no appsettings.json

Adicione a seguinte seção no arquivo `appsettings.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "seu-email@gmail.com",
    "SmtpPassword": "sua-senha-de-app",
    "FromEmail": "seu-email@gmail.com",
    "FromName": "NFSe 2026"
  }
}
```

## Configuração para Gmail

Para usar o Gmail como servidor SMTP:

1. **Habilite a verificação em duas etapas** na sua conta Google
2. **Crie uma Senha de App**:
   - Acesse: https://myaccount.google.com/apppasswords
   - Selecione "E-mail" e "Outro (nome personalizado)"
   - Digite "NFSe 2026" como nome
   - Copie a senha gerada (16 caracteres)
3. Use essa senha no campo `SmtpPassword`

Exemplo completo:
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "seu-email@gmail.com",
    "SmtpPassword": "abcd efgh ijkl mnop",
    "FromEmail": "seu-email@gmail.com",
    "FromName": "NFSe 2026"
  }
}
```

## Configuração para Outlook/Hotmail

```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": "587",
    "SmtpUser": "seu-email@outlook.com",
    "SmtpPassword": "sua-senha",
    "FromEmail": "seu-email@outlook.com",
    "FromName": "NFSe 2026"
  }
}
```

## Configuração para outros provedores

- **Yahoo Mail**: `smtp.mail.yahoo.com` (porta 587 ou 465)
- **Servidor SMTP personalizado**: Consulte a documentação do seu provedor

## Importante

⚠️ **NÃO commite o arquivo `appsettings.json` com senhas reais no Git!**

Para desenvolvimento, use variáveis de ambiente ou o arquivo `appsettings.Development.json` que está no `.gitignore`.

## Modo de Desenvolvimento (Sem Email)

Se você não configurar o email, o sistema continuará funcionando, mas apenas **logará o código de validação no console/logs** em vez de enviá-lo por email. Isso é útil para desenvolvimento e testes.

Para ver o código de validação, verifique os logs da aplicação.



