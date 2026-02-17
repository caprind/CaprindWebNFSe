# 🔍 Como Testar a Conexão com o Banco de Dados MySQL

## 📋 Métodos de Teste

### Método 1: Via MySQL Command Line (Mais Simples)

1. **Abra o terminal/prompt de comando**

2. **Teste conexão básica:**
   ```bash
   mysql -u root -p
   ```
   Digite sua senha quando solicitado.

3. **Verifique se o banco existe:**
   ```sql
   SHOW DATABASES LIKE 'NFSe2026';
   ```

4. **Conecte ao banco:**
   ```sql
   USE NFSe2026;
   SHOW TABLES;
   ```

5. **Ou use o script de teste:**
   ```bash
   mysql -u root -p < database/TestarConexao.sql
   ```

### Método 2: Via Script Batch (Windows)

Execute o arquivo:
```bash
database\TestarConexao.bat
```

Este script testa automaticamente:
- ✓ Conexão MySQL
- ✓ Acesso ao banco NFSe2026
- ✓ Lista de tabelas

### Método 3: Via Aplicação .NET (Recomendado)

1. **Configure a connection string** em `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;"
     }
   }
   ```

2. **Execute a aplicação:**
   ```bash
   cd NFSe2026.API
   dotnet run
   ```

   A aplicação tentará conectar automaticamente e:
   - Se em **Development**: Criará o banco automaticamente se não existir
   - Mostrará erros de conexão no console/logs

3. **Verifique os logs:**
   - Procure por: "Database verified/created successfully"
   - Ou erros de conexão

### Método 4: Via MySQL Workbench (GUI)

1. Abra o MySQL Workbench
2. Crie uma nova conexão:
   - Host: `localhost`
   - Port: `3306`
   - Username: `root`
   - Password: `sua_senha`
3. Clique em "Test Connection"
4. Se conectar, expanda "Schemas" e verifique se `NFSe2026` aparece

### Método 5: Teste Completo via Código

Adicione um endpoint de teste (temporário) ou execute:

```bash
cd NFSe2026.API
dotnet run
```

A aplicação tentará conectar na inicialização.

## ✅ Checklist de Verificação

Antes de testar, verifique:

- [ ] MySQL está instalado e rodando
- [ ] Serviço MySQL está ativo
- [ ] Connection string está correta no `appsettings.json`
- [ ] Usuário e senha estão corretos
- [ ] Porta MySQL está correta (padrão: 3306)
- [ ] Banco de dados foi criado (ou será criado automaticamente)

## 🔧 Verificar se MySQL está Rodando

### Windows:
```bash
net start MySQL80
# ou
sc query MySQL80
```

### Linux/Mac:
```bash
sudo systemctl status mysql
# ou
sudo service mysql status
```

## 🐛 Troubleshooting

### Erro: "Unable to connect to any of the specified MySQL hosts"

**Causas possíveis:**
1. MySQL não está rodando
   - **Solução**: Inicie o serviço MySQL
2. Host/Porta incorretos
   - **Solução**: Verifique `Server=localhost` e `Port=3306`
3. Firewall bloqueando
   - **Solução**: Configure firewall para permitir porta 3306

### Erro: "Access denied for user"

**Causas possíveis:**
1. Usuário ou senha incorretos
   - **Solução**: Verifique no `appsettings.json`
2. Usuário não tem permissões
   - **Solução**: 
     ```sql
     GRANT ALL PRIVILEGES ON NFSe2026.* TO 'root'@'localhost';
     FLUSH PRIVILEGES;
     ```

### Erro: "Unknown database 'NFSe2026'"

**Causa:** Banco de dados não existe

**Solução:**
```bash
# Criar o banco
mysql -u root -p < database/ScriptCompleto.sql
```

Ou execute no MySQL:
```sql
CREATE DATABASE IF NOT EXISTS NFSe2026 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;
```

### Erro: "Table 'xxx' doesn't exist"

**Causa:** Banco existe mas tabelas não foram criadas

**Solução:**
```bash
# Executar script completo
mysql -u root -p < database/ScriptCompleto.sql
```

Ou usar Migrations:
```bash
cd NFSe2026.API
dotnet ef database update
```

## 📝 Exemplo de Connection String Válida

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=MinhaSenha123;Port=3306;"
  }
}
```

### Para servidor remoto:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.1.100;Database=NFSe2026;User=usuario;Password=senha;Port=3306;"
  }
}
```

## 🎯 Teste Rápido (1 minuto)

1. Abra terminal
2. Execute:
   ```bash
   mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS NFSe2026; USE NFSe2026; SHOW TABLES;"
   ```
3. Se não der erro, conexão está OK!

## 📊 Verificar Status Detalhado

```sql
-- Conectar ao MySQL
mysql -u root -p

-- Verificar versão
SELECT VERSION();

-- Verificar bancos
SHOW DATABASES;

-- Selecionar banco
USE NFSe2026;

-- Verificar tabelas
SHOW TABLES;

-- Verificar estrutura de uma tabela
DESCRIBE Empresas;

-- Verificar charset
SHOW CREATE DATABASE NFSe2026;
```

