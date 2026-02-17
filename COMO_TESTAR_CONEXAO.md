# 🧪 Como Testar a Conexão MySQL

## ❌ Erro ao Executar Script

Se você recebeu: `The module 'database' could not be loaded`

Isso acontece porque no PowerShell precisa usar `.\` antes do caminho.

## ✅ Forma Correta de Executar

### Opção 1: No PowerShell

```powershell
cd "C:\Projetos IA\NFSe 2026"
.\database\TestarConexaoAtual.bat
```

**Importante:** Use `.\` antes de `database`

### Opção 2: No CMD (Prompt de Comando)

1. Abra o **Prompt de Comando** (cmd.exe)
2. Execute:

```cmd
cd "C:\Projetos IA\NFSe 2026"
database\TestarConexaoAtual.bat
```

### Opção 3: Clique Duplo

1. Navegue até: `C:\Projetos IA\NFSe 2026\database\`
2. Clique duas vezes em `TestarConexaoAtual.bat`

## 🎯 Método Mais Simples (Recomendado)

**Se você não tem MySQL instalado** ou quer testar direto pela aplicação:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run
```

A aplicação tentará conectar automaticamente e mostrará o resultado.

## 📋 Connection String Configurada

```
Server=nfs226.mysql.dbaas.com.br
Database=nfs226
User=nfs226
Password=C@p0902loc
Port=3306
```

## 🔍 Teste Direto (Se tiver MySQL instalado)

No terminal (PowerShell ou CMD):

```bash
mysql -h nfs226.mysql.dbaas.com.br -u nfs226 -pC@p0902loc nfs226 -e "SELECT 'Conexão OK!' as Status, DATABASE() as Banco, NOW() as DataHora;"
```

## ❓ Preciso ter MySQL instalado?

**Não necessariamente!** Você pode testar apenas executando a aplicação:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run
```

A aplicação tentará conectar e mostrará se funcionou ou não.

