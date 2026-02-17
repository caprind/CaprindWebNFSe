# 🗄️ Criação do Banco de Dados MySQL - NFSe 2026

## 📋 Resumo

Scripts e instruções para criar o banco de dados MySQL do sistema NFSe 2026.

## 🚀 Método Rápido (Recomendado)

### Usando Script SQL Completo

```bash
mysql -u root -p < database/ScriptCompleto.sql
```

Isso cria:
- ✅ Banco de dados `NFSe2026`
- ✅ Todas as 7 tabelas
- ✅ Todos os índices e relacionamentos
- ✅ Dados iniciais (configurações da API)

## 📁 Arquivos Disponíveis

Todos os arquivos estão na pasta `database/`:

1. **ScriptCriacaoBanco.sql** - Cria apenas o banco (sem tabelas)
2. **ScriptCompleto.sql** - Cria tudo (banco + tabelas + estrutura completa)
3. **INSTRUCOES_CRIACAO.md** - Guia completo com todos os métodos
4. **README.md** - Resumo rápido

## 📊 Estrutura Criada

O banco terá as seguintes tabelas:

| Tabela | Descrição |
|--------|-----------|
| `Empresas` | Cadastro de empresas (multi-tenancy) |
| `Usuarios` | Usuários do sistema |
| `Prestadores` | Prestadores de serviço |
| `Tomadores` | Tomadores de serviço |
| `NotasFiscais` | Notas fiscais emitidas |
| `ItensServico` | Itens de serviço |
| `ConfiguracoesAPI` | Configurações da API Nacional |

## ⚙️ Configuração

Após criar o banco, configure a connection string em:
`NFSe2026.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NFSe2026;User=root;Password=sua_senha;Port=3306;"
  }
}
```

## ✅ Próximos Passos

1. ✅ Banco de dados criado
2. ⬜ Configurar connection string
3. ⬜ Executar aplicação (ou migrations)
4. ⬜ Testar conexão

## 📚 Mais Informações

Consulte `database/INSTRUCOES_CRIACAO.md` para:
- Métodos alternativos
- Troubleshooting completo
- Configurações avançadas
- Entity Framework Core Migrations

