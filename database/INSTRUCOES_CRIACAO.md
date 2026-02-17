# Instruções para Criação do Banco de Dados MySQL

## 📋 Pré-requisitos

- MySQL Server 8.0 ou superior instalado e rodando
- Acesso ao MySQL com privilégios de CREATE DATABASE

## 🔧 Métodos de Criação

### Método 1: Script SQL Manual (Recomendado para controle total)

1. **Acesse o MySQL:**
   ```bash
   mysql -u root -p
   ```

2. **Execute o script completo:**
   ```bash
   mysql -u root -p < database/ScriptCompleto.sql
   ```

   Ou dentro do MySQL:
   ```sql
   source C:/Projetos IA/NFSe 2026/database/ScriptCompleto.sql
   ```

3. **Verifique se o banco foi criado:**
   ```sql
   SHOW DATABASES;
   USE NFSe2026;
   SHOW TABLES;
   ```

### Método 2: Entity Framework Core Migrations (Recomendado para desenvolvimento)

1. **Configure a connection string** no arquivo `NFSe2026.API/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;"
     }
   }
   ```

2. **Crie o banco de dados primeiro** (opcional, mas recomendado):
   ```sql
   CREATE DATABASE NFSe2026 CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

3. **Execute as migrations:**
   ```bash
   cd NFSe2026.API
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

   Se ainda não tiver instalado as ferramentas do EF Core:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

### Método 3: Criação Automática (Desenvolvimento)

1. **Configure a connection string** no `appsettings.json`

2. **Execute a aplicação:**
   ```bash
   cd NFSe2026.API
   dotnet run
   ```

   O banco será criado automaticamente usando `EnsureCreated()` (apenas em modo Development).

## ⚙️ Configuração da Connection String

Edite `NFSe2026.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;"
  }
}
```

### Parâmetros da Connection String:

- **Server**: Endereço do servidor MySQL (localhost ou IP)
- **Database**: Nome do banco de dados (NFSe2026)
- **User**: Usuário do MySQL
- **Password**: Senha do usuário
- **Port**: Porta do MySQL (padrão: 3306)

## 📊 Estrutura das Tabelas

O banco de dados será criado com as seguintes tabelas:

1. **Empresas** - Cadastro de empresas
2. **Usuarios** - Usuários do sistema (vinculados a empresas)
3. **Prestadores** - Prestadores de serviço (vinculados a empresas)
4. **Tomadores** - Tomadores de serviço
5. **NotasFiscais** - Notas fiscais emitidas
6. **ItensServico** - Itens de serviço das notas fiscais
7. **ConfiguracoesAPI** - Configurações da API Nacional de NFS-e

## ✅ Verificação

Após criar o banco, verifique:

```sql
USE NFSe2026;

-- Listar todas as tabelas
SHOW TABLES;

-- Verificar estrutura de uma tabela
DESCRIBE Empresas;
DESCRIBE Usuarios;
DESCRIBE Prestadores;

-- Contar registros
SELECT COUNT(*) FROM Empresas;
SELECT COUNT(*) FROM Usuarios;
```

## 🔍 Troubleshooting

### Erro: "Access denied for user"
- Verifique usuário e senha
- Verifique se o usuário tem privilégios necessários:
  ```sql
  GRANT ALL PRIVILEGES ON NFSe2026.* TO 'seu_usuario'@'localhost';
  FLUSH PRIVILEGES;
  ```

### Erro: "Unknown database"
- Certifique-se de que o banco foi criado:
  ```sql
  CREATE DATABASE IF NOT EXISTS NFSe2026 CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
  ```

### Erro: "Table already exists"
- Se usar o script SQL manual, ele usa `CREATE TABLE IF NOT EXISTS`, então é seguro executar novamente
- Para migrations, use `dotnet ef database update` para atualizar

### Erro de Charset
- Certifique-se de usar `utf8mb4` para suportar caracteres especiais e emojis
- Verifique o charset do banco:
  ```sql
  SHOW CREATE DATABASE NFSe2026;
  ```

### Erro de Conexão no EF Core
- Verifique se o MySQL está rodando:
  ```bash
  # Windows
  net start MySQL80
  
  # Linux/Mac
  sudo systemctl status mysql
  ```
- Teste a conexão:
  ```bash
  mysql -u root -p -h localhost
  ```

## 🔄 Atualizações Futuras

Quando adicionar novos modelos ou alterar estruturas:

1. **Usando Migrations:**
   ```bash
   dotnet ef migrations add NomeDaMigration
   dotnet ef database update
   ```

2. **Manualmente:**
   - Edite o script SQL
   - Execute as alterações no banco

## 📝 Notas Importantes

- ⚠️ **Produção**: Em produção, use migrations do EF Core ao invés de `EnsureCreated()`
- 🔐 **Segurança**: Não commite senhas no código. Use variáveis de ambiente ou User Secrets
- 💾 **Backup**: Faça backup regular do banco de dados
- 🚀 **Performance**: Os índices já estão configurados nas tabelas principais

## 📚 Referências

- [MySQL Documentation](https://dev.mysql.com/doc/)
- [Entity Framework Core - MySQL](https://docs.microsoft.com/en-us/ef/core/providers/pomelo/)
- [Pomelo EF Core MySQL](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)

