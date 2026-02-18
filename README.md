# CAPRINDWEB - Sistema de Emissão de NFS-e

Sistema ASP.NET Core para emissão de Nota Fiscal de Serviços Eletrônica (NFS-e) integrado com a API Nacional, utilizando MySQL como banco de dados.

## 🚀 Tecnologias

- **ASP.NET Core 8.0**
- **Entity Framework Core** com **Pomelo.EntityFrameworkCore.MySql**
- **MySQL**
- **AutoMapper** para mapeamento de objetos
- **FluentValidation** para validação
- **Serilog** para logging
- **Swagger/OpenAPI** para documentação da API

## 📋 Pré-requisitos

- .NET 8.0 SDK ou superior
- MySQL Server 8.0 ou superior
- Visual Studio 2022, VS Code ou Rider (opcional)

## ⚙️ Configuração

### 1. Banco de Dados

1. Crie um banco de dados MySQL:
```sql
CREATE DATABASE NFSe2026 CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

2. Atualize a connection string no arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;"
  }
}
```

### 2. Configuração da API Nacional

Configure as credenciais da API Nacional no `appsettings.json`:

```json
{
  "ApiNacionalNFSe": {
    "UrlBase": "https://api-homologacao.nfse.gov.br",
    "ClientId": "seu_client_id",
    "ClientSecret": "seu_client_secret",
    "Scope": "nfse",
    "Timeout": 30,
    "Ambiente": "Homologacao"
  }
}
```

### 3. Migrações do Banco de Dados

Execute as migrações para criar as tabelas:

```bash
cd NFSe2026.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Ou, se preferir, o banco será criado automaticamente em desenvolvimento na primeira execução (usando `EnsureCreated`).

## 🏃 Executando o Projeto

```bash
cd NFSe2026.API
dotnet run
```

A API estará disponível em:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger**: http://localhost:5000/swagger (em desenvolvimento)

## 📚 Endpoints da API

### Prestador
- `GET /api/prestador` - Lista todos os prestadores
- `GET /api/prestador/{id}` - Obtém prestador por ID
- `POST /api/prestador` - Cria um novo prestador
- `PUT /api/prestador/{id}` - Atualiza prestador
- `DELETE /api/prestador/{id}` - Desativa prestador

### Tomador
- `GET /api/tomador` - Lista todos os tomadores
- `GET /api/tomador/{id}` - Obtém tomador por ID
- `POST /api/tomador` - Cria um novo tomador
- `PUT /api/tomador/{id}` - Atualiza tomador
- `DELETE /api/tomador/{id}` - Remove tomador

### Nota Fiscal
- `GET /api/notafiscal` - Lista todas as notas fiscais (opcional: ?prestadorId={id})
- `GET /api/notafiscal/{id}` - Obtém nota fiscal por ID
- `POST /api/notafiscal` - Emite uma nova nota fiscal
- `POST /api/notafiscal/{id}/cancelar` - Cancela uma nota fiscal
- `GET /api/notafiscal/{id}/consultar` - Consulta situação na API Nacional
- `GET /api/notafiscal/{id}/xml` - Obtém XML da nota fiscal

## 📁 Estrutura do Projeto

```
NFSe2026.API/
├── Controllers/          # Controllers da API
├── Data/                 # DbContext e configurações do EF Core
├── DTOs/                 # Data Transfer Objects
├── Models/               # Modelos de domínio (entidades)
├── Services/             # Serviços de negócio e integração
├── Configurations/       # Classes de configuração
├── Mappings/             # Perfis do AutoMapper
└── Middlewares/          # Middlewares customizados
```

## 🔐 Segurança

**Nota**: Este é um projeto base. Para produção, considere implementar:

- Autenticação e autorização (JWT)
- Criptografia de dados sensíveis (certificados, senhas)
- Validação de entrada mais robusta
- Rate limiting
- HTTPS obrigatório
- CORS configurado adequadamente

## 📝 Notas Importantes

1. **API Nacional**: Os endpoints e estruturas de dados da API Nacional são exemplos genéricos. É necessário ajustar conforme a documentação oficial da API Nacional de NFS-e.

2. **Ambiente**: O projeto está configurado para ambiente de homologação por padrão. Altere para produção quando necessário.

3. **Logging**: Os logs são salvos em `logs/nfse-YYYYMMDD.txt` e também exibidos no console.

4. **Certificados Digitais**: Atualmente, os certificados são armazenados como string. Para produção, considere usar um serviço de gerenciamento de segredos ou criptografar os dados.

## 🧪 Testes

Para adicionar testes, crie um projeto de testes:

```bash
dotnet new xunit -n NFSe2026.Tests
dotnet sln add NFSe2026.Tests/NFSe2026.Tests.csproj
cd NFSe2026.Tests
dotnet add reference ../NFSe2026.API/NFSe2026.API.csproj
```

## 📄 Licença

Este projeto é fornecido como está, para fins educacionais e de desenvolvimento.

## 🤝 Contribuindo

Sinta-se à vontade para fazer fork, criar issues ou enviar pull requests.

