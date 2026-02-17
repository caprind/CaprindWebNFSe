# Banco de Dados - NFSe 2026

## 📋 Scripts Disponíveis

| Arquivo | Descrição |
|---------|-----------|
| **ScriptCriacaoBanco.sql** | Cria apenas o banco de dados (sem tabelas) |
| **ScriptCompleto.sql** | Cria o banco e todas as tabelas com estrutura completa |
| **INSTRUCOES_CRIACAO.md** | Instruções detalhadas passo a passo |

## 🚀 Criação Rápida

### Opção 1: Script SQL Completo (Mais Rápido - Recomendado)

```bash
# Criar banco + todas as tabelas de uma vez
mysql -u root -p < database/ScriptCompleto.sql
```

Ou usando MySQL Workbench:
1. Abra o MySQL Workbench
2. File → Open SQL Script → Selecione `ScriptCompleto.sql`
3. Execute o script (⚡ Execute)

### Opção 2: Criar apenas o banco (depois usar Migrations)

```bash
# Passo 1: Criar apenas o banco
mysql -u root -p < database/ScriptCriacaoBanco.sql

# Passo 2: Configurar connection string no appsettings.json
# Passo 3: Executar migrations (veja Opção 3 abaixo)
```

### Opção 3: Entity Framework Core Migrations (Recomendado para desenvolvimento)

1. **Configure a connection string** no `NFSe2026.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;"
     }
   }
   ```

2. **Instale as ferramentas do EF Core** (se ainda não tiver):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **Crie e aplique as migrations**:
   ```bash
   cd NFSe2026.API
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

### Opção 4: Criação Automática (Apenas Desenvolvimento)

O banco será criado automaticamente quando você executar a aplicação em modo Development:

```bash
cd NFSe2026.API
dotnet run
```

⚠️ **Nota**: Este método usa `EnsureCreated()` que não cria migrations. Use apenas para desenvolvimento inicial.

## 📊 Estrutura do Banco

O banco contém as seguintes tabelas:

- **Empresas** - Cadastro de empresas
- **Usuarios** - Usuários do sistema (vinculados a empresas)
- **Prestadores** - Prestadores de serviço (multi-tenancy por empresa)
- **Tomadores** - Tomadores de serviço
- **NotasFiscais** - Notas fiscais emitidas
- **ItensServico** - Itens de serviço das notas fiscais
- **ConfiguracoesAPI** - Configurações da API Nacional de NFS-e

## ⚙️ Configuração da Connection String

Edite `NFSe2026.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;"
  }
}
```

### Parâmetros:

- **Server**: `localhost` ou IP do servidor MySQL
- **Database**: `NFSe2026`
- **User**: Seu usuário MySQL (ex: `root`)
- **Password**: Sua senha MySQL
- **Port**: `3306` (padrão)

## ✅ Verificação

Após criar o banco, verifique:

```sql
USE NFSe2026;

-- Listar tabelas
SHOW TABLES;

-- Verificar estrutura
DESCRIBE Empresas;
DESCRIBE Usuarios;
DESCRIBE Prestadores;

-- Contar registros
SELECT COUNT(*) FROM Empresas;
```

## 📚 Documentação Completa

Para instruções detalhadas, troubleshooting e mais opções, consulte:
- **[INSTRUCOES_CRIACAO.md](INSTRUCOES_CRIACAO.md)** - Guia completo
