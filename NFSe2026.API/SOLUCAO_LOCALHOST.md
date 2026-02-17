# 🔧 Solução: Erro "localhost" ao invés do servidor correto

## ❌ Problema Identificado

O erro mostrava:
```
[20:00:10 ERR] An error occurred using the connection to database '' on server 'localhost'.
```

## 🔍 Causa

O arquivo `appsettings.Development.json` estava sobrescrevendo a connection string do `appsettings.json` com uma connection string apontando para `localhost`.

O ASP.NET Core carrega as configurações na seguinte ordem (último sobrescreve):
1. `appsettings.json`
2. `appsettings.{Environment}.json` (ex: `appsettings.Development.json`)

Como a aplicação estava rodando em modo **Development**, o `appsettings.Development.json` estava sobrescrevendo a connection string correta.

## ✅ Solução Aplicada

Atualizado o `appsettings.Development.json` para usar a mesma connection string do `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=nfs226.mysql.dbaas.com.br;Database=nfs226;User=nfs226;Password=C@p0902loc;Port=3306;SslMode=Preferred;ConnectionTimeout=60;DefaultCommandTimeout=60;AllowUserVariables=True;"
  }
}
```

## 🚀 Próximos Passos

Agora execute novamente:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run
```

A aplicação deve tentar conectar ao servidor correto: `nfs226.mysql.dbaas.com.br`

## 💡 Dica

Se você precisar de connection strings diferentes para Development e Production:

- **Development**: Mantenha no `appsettings.Development.json`
- **Production**: Use variáveis de ambiente ou `appsettings.Production.json`

Para não usar Development, rode:
```powershell
dotnet run --environment Production
```

