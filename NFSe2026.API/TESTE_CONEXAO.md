# 🧪 Teste de Conexão - Banco de Dados

## Método Rápido

### 1. Via Terminal MySQL

```bash
mysql -u root -p
```

Depois execute:
```sql
USE NFSe2026;
SHOW TABLES;
SELECT COUNT(*) FROM Empresas;
```

### 2. Via Aplicação

```bash
cd NFSe2026.API
dotnet run
```

A aplicação tentará conectar na inicialização.

## Verificar Configuração

Edite `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=SUA_SENHA_AQUI;Port=3306;"
  }
}
```

## Troubleshooting

- ❌ **Não conecta?** Verifique se MySQL está rodando
- ❌ **Erro de acesso?** Verifique usuário e senha
- ❌ **Banco não existe?** Execute: `mysql -u root -p < database/ScriptCompleto.sql`

