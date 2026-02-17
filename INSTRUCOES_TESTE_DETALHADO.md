# 🔍 Instruções: Teste Detalhado de Conexão

## ✅ O que foi criado

Foi criado um **teste detalhado de conexão** que será executado automaticamente quando você iniciar a aplicação em modo Development.

## 📋 O que o teste faz

O teste realiza 3 tipos de verificações:

### 1. **Conexão Direta com MySqlConnection**
- Testa conexão básica ao MySQL
- Mostra versão do MySQL, banco atual, usuário e hora do servidor
- Identifica erros de conexão em nível baixo

### 2. **Teste com Diferentes Modos SSL**
- Testa `SslMode=None` (sem SSL)
- Testa `SslMode=Preferred` (SSL preferencial)
- Testa `SslMode=Required` (SSL obrigatório)
- Identifica qual modo SSL funciona com seu servidor

### 3. **Teste com Entity Framework Core**
- Testa conexão através do DbContext
- Verifica se o EF Core consegue conectar
- Identifica problemas específicos do EF Core

## 🚀 Como Executar

### Opção 1: Executar a Aplicação (Recomendado)

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run
```

O teste será executado automaticamente no início e mostrará os resultados no console.

### Opção 2: Se a Aplicação Já Está Rodando

1. Pare a aplicação (Ctrl+C)
2. Execute novamente: `dotnet run`

## 📊 Como Interpretar os Resultados

### ✅ SUCESSO
Se algum teste mostrar `✅ SUCESSO`, significa que a conexão funciona com aquela configuração.

**Ação:**
- Se o Teste 1 funcionou mas o Teste 3 não, há problema na configuração do EF Core
- Se algum SslMode funcionou, atualize `appsettings.json` com aquele modo

### ❌ ERRO
Se todos os testes falharem, verifique:

1. **Erro: "Access denied"**
   - Credenciais incorretas (usuário/senha)
   - Usuário não tem permissão no banco

2. **Erro: "Unable to connect"**
   - IP não está na whitelist
   - Firewall bloqueando
   - Servidor offline

3. **Erro: "SSL" ou "TLS"**
   - Problema com certificado SSL
   - Tente outro SslMode

4. **Erro: "Unknown database"**
   - Banco de dados não existe
   - Nome do banco está errado

## 🔧 Correções Baseadas no Teste

### Se SslMode=None funcionou:
Edite `appsettings.json`:
```json
"DefaultConnection": "...;SslMode=None;..."
```

### Se SslMode=Required funcionou:
Edite `appsettings.json`:
```json
"DefaultConnection": "...;SslMode=Required;..."
```

### Se conexão direta funcionou mas EF Core não:
- Verifique se está usando a mesma connection string
- Verifique a versão do MySQL no `Program.cs` (deve ser `8.0.0-mysql`)

## 📝 Exemplo de Saída Esperada

```
============================================================
TESTE DETALHADO DE CONEXÃO MySQL
============================================================

Connection String: Server=nfs226.mysql.dbaas.com.br;
Database=nfs226;
User=nfs226;
Password=***;
Port=3306;
SslMode=Preferred;
ConnectionTimeout=60;

TESTE 1: Conexão direta com MySqlConnection
------------------------------------------------------------
✅ SUCESSO: Conexão direta funcionou!
   MySQL Version: 8.0.xx
   Database: nfs226
   User: nfs226@xxx.xxx.xxx.xxx
   Server Time: 2024-01-XX XX:XX:XX

TESTE 2.1: Teste com SslMode=None
------------------------------------------------------------
❌ ERRO com SslMode=None: ...

TESTE 2.2: Teste com SslMode=Preferred
------------------------------------------------------------
✅ SUCESSO com SslMode=Preferred!

TESTE 3: Teste com Entity Framework Core
------------------------------------------------------------
✅ SUCESSO: DbContext conseguiu conectar!
✅ Banco atual: nfs226
============================================================
FIM DOS TESTES
============================================================
```

## 🆘 Se Nada Funcionar

1. Entre em contato com o provedor de hospedagem
2. Informe:
   - Servidor: `nfs226.mysql.dbaas.com.br`
   - Porta: `3306`
   - Erro específico mostrado no teste
3. Pergunte sobre:
   - Status do servidor MySQL
   - Necessidade de whitelist de IP
   - Configurações SSL recomendadas
   - Se as credenciais estão corretas

