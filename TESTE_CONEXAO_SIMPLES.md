# 🧪 Como Testar a Conexão MySQL

## 📋 Connection String Configurada

```
Server=nfs226.mysql.dbaas.com.br;Database=nfs226;User=nfs226;Password=C@p0902loc;Port=3306;
```

## ✅ Métodos de Teste

### Método 1: Via PowerShell (Recomendado)

No PowerShell, execute:

```powershell
cd "C:\Projetos IA\NFSe 2026"
.\database\TestarConexaoAtual.bat
```

Ou diretamente:

```powershell
mysql -h nfs226.mysql.dbaas.com.br -u nfs226 -pC@p0902loc nfs226 -e "SELECT 'Conexão OK!' as Status, DATABASE() as Banco, NOW() as DataHora;"
```

### Método 2: Via CMD (Prompt de Comando)

1. Abra o **Prompt de Comando** (não PowerShell)
2. Execute:

```cmd
cd "C:\Projetos IA\NFSe 2026"
database\TestarConexaoAtual.bat
```

### Método 3: Via MySQL Command Line Direto

Abra o terminal e execute:

```bash
mysql -h nfs226.mysql.dbaas.com.br -u nfs226 -pC@p0902loc nfs226
```

Depois execute:
```sql
SELECT 'Conexão OK!' as Status, DATABASE() as Banco, NOW() as DataHora;
SHOW TABLES;
```

### Método 4: Via Aplicação

Execute a aplicação:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run
```

A aplicação tentará conectar automaticamente.

## 🔍 Verificações Rápidas

### 1. Servidor está acessível?
```powershell
ping nfs226.mysql.dbaas.com.br
```

### 2. Porta 3306 está aberta?
```powershell
Test-NetConnection nfs226.mysql.dbaas.com.br -Port 3306
```

### 3. MySQL está instalado no seu computador?
```bash
mysql --version
```

Se não tiver MySQL instalado, você precisará instalar o MySQL Client ou usar outro método de teste.

## ❌ Se Der Erro

### Erro: "mysql: command not found"
- **Causa:** MySQL Client não está instalado
- **Solução:** 
  - Instale o MySQL Client, ou
  - Use o teste via aplicação (`dotnet run`)

### Erro: "Unable to connect"
- **Causa:** Servidor não está acessível
- **Soluções:**
  - Verifique se o servidor está online
  - Verifique firewall
  - Verifique se precisa de whitelist de IP

## 🎯 Teste Mais Simples (Sem MySQL Client)

Se você não tiver MySQL instalado, o **melhor método** é executar a aplicação:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run
```

A aplicação tentará conectar e mostrará o resultado no console.

