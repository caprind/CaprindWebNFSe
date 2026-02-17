# 🔧 Troubleshooting - Erro de Conexão MySQL

## ❌ Erro: "Unable to connect to any of the specified MySQL hosts"

Este erro indica que a aplicação não consegue conectar ao servidor MySQL.

## 🔍 Diagnóstico Passo a Passo

### 1. Verificar Connection String

Sua connection string atual:
```
Server=mysql02.caprind1.hospedagemdesites.ws;Database=NFSe2026;User=caprind11;Password=cap0902loc;Port=3306;
```

### 2. Testar Conexão Manualmente

Execute no terminal:
```bash
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -pcap0902loc -e "SELECT 1"
```

**Se funcionar:** O problema está na aplicação
**Se não funcionar:** O problema está na conexão/servidor

### 3. Verificações Básicas

#### ✅ O servidor MySQL está acessível?
```bash
ping mysql02.caprind1.hospedagemdesites.ws
```

#### ✅ A porta 3306 está acessível?
```bash
telnet mysql02.caprind1.hospedagemdesites.ws 3306
```

Ou no PowerShell:
```powershell
Test-NetConnection mysql02.caprind1.hospedagemdesites.ws -Port 3306
```

#### ✅ As credenciais estão corretas?
- Verifique usuário: `caprind11`
- Verifique senha: `cap0902loc`
- Verifique se o usuário tem acesso ao banco `NFSe2026`

#### ✅ O banco de dados existe?
```bash
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -pcap0902loc -e "SHOW DATABASES LIKE 'NFSe2026'"
```

### 4. Possíveis Soluções

#### Solução 1: Verificar Firewall/Antivírus
- Desative temporariamente o firewall
- Adicione exceção para porta 3306
- Verifique se o antivírus não está bloqueando

#### Solução 2: Adicionar Parâmetros na Connection String

Tente adicionar parâmetros extras:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql02.caprind1.hospedagemdesites.ws;Database=NFSe2026;User=caprind11;Password=cap0902loc;Port=3306;SslMode=None;ConnectionTimeout=30;"
  }
}
```

Ou com SSL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql02.caprind1.hospedagemdesites.ws;Database=NFSe2026;User=caprind11;Password=cap0902loc;Port=3306;SslMode=Preferred;ConnectionTimeout=30;"
  }
}
```

#### Solução 3: Verificar Permissões do Usuário

No servidor MySQL, verifique se o usuário tem permissões:
```sql
SHOW GRANTS FOR 'caprind11'@'%';
```

Se necessário, conceda permissões:
```sql
GRANT ALL PRIVILEGES ON NFSe2026.* TO 'caprind11'@'%';
FLUSH PRIVILEGES;
```

#### Solução 4: Criar Banco de Dados

Se o banco não existe, crie-o:
```bash
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -pcap0902loc -e "CREATE DATABASE IF NOT EXISTS NFSe2026 CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci"
```

#### Solução 5: Usar IP ao invés de Hostname

Se o hostname não resolve, tente descobrir o IP:
```bash
nslookup mysql02.caprind1.hospedagemdesites.ws
```

E use o IP na connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=XXX.XXX.XXX.XXX;Database=NFSe2026;User=caprind11;Password=cap0902loc;Port=3306;"
  }
}
```

#### Solução 6: Verificar Configuração do Servidor MySQL

O servidor MySQL pode estar configurado para aceitar conexões apenas de IPs específicos. Verifique com o provedor de hospedagem.

## 🧪 Script de Teste Completo

Crie um arquivo `TestConnection.ps1`:

```powershell
Write-Host "Testando conexão MySQL..." -ForegroundColor Yellow

# Teste 1: Ping
Write-Host "`n1. Testando ping..." -ForegroundColor Cyan
Test-Connection mysql02.caprind1.hospedagemdesites.ws -Count 2

# Teste 2: Porta
Write-Host "`n2. Testando porta 3306..." -ForegroundColor Cyan
Test-NetConnection mysql02.caprind1.hospedagemdesites.ws -Port 3306

# Teste 3: MySQL
Write-Host "`n3. Testando conexão MySQL..." -ForegroundColor Cyan
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -pcap0902loc -e "SELECT 'Conexão OK!' as Status, NOW() as DataHora"
```

## 🔐 Connection String Otimizada

Tente esta connection string com mais parâmetros:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql02.caprind1.hospedagemdesites.ws;Database=NFSe2026;User=caprind11;Password=cap0902loc;Port=3306;SslMode=Preferred;ConnectionTimeout=60;DefaultCommandTimeout=60;AllowUserVariables=True;UseAffectedRows=False;"
  }
}
```

## 📞 Próximos Passos

1. ✅ Execute o teste manual via `mysql` command line
2. ✅ Verifique se o servidor está acessível (ping/telnet)
3. ✅ Teste diferentes connection strings
4. ✅ Entre em contato com o provedor de hospedagem se o servidor for remoto
5. ✅ Verifique se precisa de VPN ou IP whitelist

## 🆘 Se Nada Funcionar

1. Verifique com o provedor de hospedagem:
   - Se o MySQL está ativo
   - Se sua conexão está permitida
   - Se precisa configurar IP whitelist
   - Se há alguma restrição de firewall

2. Tente conexão local:
   - Configure MySQL local para teste
   - Use: `Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;`

