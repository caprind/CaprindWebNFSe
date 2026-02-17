# Atualização: Consulta CNPJ usando Brasil API

## ✅ Alterações Realizadas

O serviço de consulta CNPJ foi atualizado para usar a **Brasil API** (brasilapi.com.br), uma API pública gratuita que consolida dados do governo brasileiro.

### Mudanças Implementadas

1. **URL da API alterada**:
   - **Antes**: `https://www.receitaws.com.br/v1/cnpj/{cnpj}`
   - **Agora**: `https://brasilapi.com.br/api/cnpj/v1/{cnpj}`

2. **Estrutura de resposta ajustada**:
   - Campos mapeados conforme estrutura da Brasil API
   - `razao_social` ao invés de `nome`
   - `nome_fantasia` (mesmo campo)
   - `descricao_situacao_cadastral` ao invés de `situacao`
   - `data_inicio_atividade` ao invés de `abertura`
   - `ddd_telefone_1` para telefone
   - `inscricoes_estaduais` (array) para inscrição estadual

3. **User-Agent adicionado**:
   - Headers HTTP incluem identificação da aplicação

### Vantagens da Brasil API

- ✅ API pública e gratuita
- ✅ Sem necessidade de autenticação
- ✅ Dados atualizados da Receita Federal
- ✅ Alta disponibilidade
- ✅ Sem limite de requisições (dentro do uso razoável)
- ✅ Documentação completa: https://brasilapi.com.br/docs

### Endpoint da API

```
GET https://brasilapi.com.br/api/cnpj/v1/{cnpj}
```

Onde `{cnpj}` é o CNPJ sem formatação (apenas números, 14 dígitos).

### Exemplo de Resposta da Brasil API

```json
{
  "cnpj": "12345678000190",
  "razao_social": "EMPRESA EXEMPLO LTDA",
  "nome_fantasia": "EXEMPLO",
  "descricao_situacao_cadastral": "ATIVA",
  "porte": "DEMAIS",
  "natureza_juridica": "2062",
  "data_inicio_atividade": "2020-01-01",
  "logradouro": "RUA EXEMPLO",
  "numero": "123",
  "complemento": "SALA 1",
  "bairro": "CENTRO",
  "municipio": "SÃO PAULO",
  "uf": "SP",
  "cep": "01000000",
  "ddd_telefone_1": "11987654321",
  "email": "contato@exemplo.com.br",
  "inscricoes_estaduais": [
    {
      "inscricao_estadual": "123456789012",
      "ativo": true,
      "estado": {
        "sigla": "SP"
      }
    }
  ]
}
```

### Campos Mapeados

| Campo no Sistema | Campo na Brasil API | Observação |
|-----------------|---------------------|------------|
| CNPJ | `cnpj` | Sem formatação |
| RazaoSocial | `razao_social` | Nome oficial da empresa |
| NomeFantasia | `nome_fantasia` | Nome de fantasia |
| SituacaoCadastral | `descricao_situacao_cadastral` | Ex: "ATIVA", "INAPTA" |
| Porte | `porte` | Ex: "DEMAIS", "MICRO", "PEQUENO" |
| NaturezaJuridica | `natureza_juridica` | Código da natureza jurídica |
| DataAbertura | `data_inicio_atividade` | Data de início das atividades |
| Endereco.Logradouro | `logradouro` | Nome da rua/avenida |
| Endereco.Numero | `numero` | Número do endereço |
| Endereco.Complemento | `complemento` | Complemento do endereço |
| Endereco.Bairro | `bairro` | Bairro |
| Endereco.Cidade | `municipio` | Município |
| Endereco.UF | `uf` | Unidade Federativa (sigla) |
| Endereco.CEP | `cep` | CEP sem formatação |
| Telefone | `ddd_telefone_1` | Telefone completo |
| Email | `email` | Email da empresa |
| InscricaoEstadual | `inscricoes_estaduais[0].inscricao_estadual` | Primeira IE do array |
| InscricaoMunicipal | `inscricao_municipal` | Quando disponível |

### Código Atualizado

O serviço `ConsultaCNPJService` agora:
- Usa a Brasil API como fonte de dados
- Mapeia corretamente todos os campos
- Trata arrays (inscrições estaduais)
- Mantém fallbacks para compatibilidade
- Loga erros adequadamente

### Documentação da API

- **Site**: https://brasilapi.com.br
- **Documentação**: https://brasilapi.com.br/docs
- **GitHub**: https://github.com/BrasilAPI/BrasilAPI
- **Status**: https://status.brasilapi.com.br

### Notas Importantes

1. **Formato do CNPJ**: Deve ser enviado sem formatação (apenas números, 14 dígitos)
2. **Rate Limiting**: A Brasil API não tem limite oficial, mas recomenda-se usar com moderação
3. **Disponibilidade**: API mantida pela comunidade, verifique status em caso de problemas
4. **Cache**: Considere implementar cache das consultas para reduzir chamadas à API

### Testes

Para testar a consulta:

```http
GET /api/empresa/consultar-cnpj/12345678000190
```

Ou durante o cadastro:

```http
POST /api/auth/cadastro
{
  "cnpj": "12.345.678/0001-90",
  "nome": "Nome do Usuário",
  "email": "email@exemplo.com",
  "senha": "SenhaSegura123"
}
```

O sistema irá:
1. Limpar a formatação do CNPJ
2. Consultar na Brasil API
3. Preencher automaticamente os dados da empresa
4. Criar o registro no banco de dados

