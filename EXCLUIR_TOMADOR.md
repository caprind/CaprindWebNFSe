# 🗑️ Guia: Excluir/Deletar Tomador

## ✅ Endpoint de Exclusão

- **URL**: `DELETE http://localhost:5215/api/tomador/{id}`
- **Autenticação**: Obrigatória (JWT Bearer Token)
- **Body**: Não necessário

## 📝 Como Usar no Postman

### 1. Pré-requisito: Estar Autenticado

Você precisa ter um token JWT válido. Faça login primeiro:
- `POST /api/auth/login`

### 2. Descobrir o ID do Tomador

Antes de excluir, você precisa saber o ID do tomador. Use um dos métodos:

**Opção 1: Listar todos os tomadores**
```
GET http://localhost:5215/api/tomador
```

**Opção 2: Buscar um tomador específico (se souber o ID)**
```
GET http://localhost:5215/api/tomador/{id}
```

### 3. Configuração no Postman

#### Passo 1: Criar a Requisição
1. Clique em "New" → "HTTP Request"
2. Ou use o botão "+" para nova aba

#### Passo 2: Configurar Método e URL
1. Selecione `DELETE` no dropdown
2. Digite: `http://localhost:5215/api/tomador/{id}`
   - Substitua `{id}` pelo ID do tomador que deseja excluir
   - Exemplo: `http://localhost:5215/api/tomador/1`

#### Passo 3: Configurar Autenticação
Na aba **"Authorization"**:
- Type: `Bearer Token`
- Token: cole o token JWT obtido no login

OU na aba **"Headers"**, adicione:
- Key: `Authorization`
- Value: `Bearer {seu_token_aqui}`

#### Passo 4: Enviar Requisição
1. Clique em "Send"
2. Verifique o status code da resposta

### 4. Exemplo de Requisição Completa

**URL:**
```
DELETE http://localhost:5215/api/tomador/1
```

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Body:** Não necessário (é um DELETE)

### 5. Resposta Esperada (204 No Content)

Quando a exclusão for bem-sucedida, a resposta será:
- **Status Code**: `204 No Content`
- **Body**: Vazio (sem conteúdo)

### 6. Possíveis Erros

#### Erro 401 - Unauthorized

Resposta:
```json
{}
```

**Causas:**
- Token não fornecido
- Token inválido/expirado
- Header Authorization mal formatado

**Solução:**
- Faça login novamente
- Verifique o header: `Authorization: Bearer {token}`

#### Erro 404 - Not Found

Resposta:
```json
{}
```

**Causa:**
- Tomador não encontrado com o ID fornecido
- Tomador já foi excluído anteriormente

**Solução:**
- Verifique se o ID está correto
- Liste os tomadores para verificar os IDs disponíveis: `GET /api/tomador`

#### Erro 500 - Internal Server Error

```json
{
  "error": "Erro interno do servidor"
}
```

**Causa:**
- Problema no servidor/banco de dados
- Possível violação de constraint (ex: tomador vinculado a notas fiscais)

**Solução:**
- Verifique se a aplicação está rodando
- Verifique os logs do servidor
- Verifique se o tomador não está sendo usado em outras entidades

### 7. Fluxo Completo: Verificar → Excluir → Confirmar

#### Passo 1: Listar Tomadores

```
GET http://localhost:5215/api/tomador

Headers:
Authorization: Bearer {seu_token}
```

Resposta (exemplo):
```json
[
  {
    "id": 1,
    "tipoPessoa": 2,
    "cpfcnpj": "11222333000181",
    "razaoSocialNome": "EMPRESA EXEMPLO LTDA",
    ...
  },
  {
    "id": 2,
    "tipoPessoa": 1,
    "cpfcnpj": "12345678901",
    "razaoSocialNome": "João da Silva",
    ...
  }
]
```

#### Passo 2: Excluir o Tomador

```
DELETE http://localhost:5215/api/tomador/1

Headers:
Authorization: Bearer {seu_token}
```

Resposta: `204 No Content`

#### Passo 3: Verificar Exclusão

```
GET http://localhost:5215/api/tomador/1

Headers:
Authorization: Bearer {seu_token}
```

Resposta esperada: `404 Not Found` (tomador não existe mais)

OU liste todos novamente:

```
GET http://localhost:5215/api/tomador

Headers:
Authorization: Bearer {seu_token}
```

O tomador excluído não aparecerá mais na lista.

### 8. Exemplo Prático Completo

#### Excluir Tomador ID 1:

```
DELETE http://localhost:5215/api/tomador/1

Headers:
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJFbXByZXNhSWQiOiIxIiwiZW1haWwiOiJqb2FvQGVtcHJlc2EuY29tLmJyIiwibmJmIjoxNzA0MjQ5ODAwLCJleHAiOjE3MDQyNzg2MDAsImlhdCI6MTcwNDI0OTgwMCwiaXNzIjoiTkZTZTIwMjYiLCJhdWQiOiJORlNlMjAyNiJ9...
```

### 9. ⚠️ Importante: Considerações sobre Exclusão

#### Exclusão Física vs Lógica

**Exclusão Física (Atual):**
- O registro é **permanentemente removido** do banco de dados
- Não pode ser recuperado
- Se o tomador estiver vinculado a notas fiscais, pode ocorrer erro de constraint

**Sugestão para o Futuro:**
- Se necessário, implementar exclusão lógica (campo `Ativo = false`)
- Isso permite "desativar" sem perder histórico de notas fiscais

#### Verificações Recomendadas

Antes de excluir, verifique:
1. ✅ O ID está correto?
2. ✅ O tomador não está sendo usado em notas fiscais?
3. ✅ Você tem certeza que deseja excluir permanentemente?

### 10. Comparação: Endpoints Disponíveis

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/tomador` | GET | Listar todos os tomadores |
| `/api/tomador/{id}` | GET | Obter tomador por ID |
| `/api/tomador` | POST | Cadastrar tomador (manual completo) |
| `/api/tomador/por-cnpj` | POST | Cadastrar tomador apenas com CNPJ |
| `/api/tomador/{id}` | PUT | Atualizar tomador |
| `/api/tomador/{id}` | DELETE | **Excluir tomador** |

### 11. Dicas Importantes

✅ **O que acontece quando você exclui:**
- O tomador é removido permanentemente do banco de dados
- Não pode ser recuperado
- Qualquer referência ao ID do tomador em outras tabelas pode causar problemas

❌ **O que NÃO acontece:**
- Notas fiscais vinculadas não são excluídas automaticamente
- Se houver constraint de foreign key, a exclusão pode falhar

🔍 **Para evitar problemas:**
- Sempre verifique se o tomador está sendo usado antes de excluir
- Considere implementar exclusão lógica se precisar manter histórico

### 12. Teste Rápido

1. **Liste os tomadores** para ver os IDs disponíveis
2. **Escolha um ID** para excluir
3. **Faça a requisição DELETE** com o ID escolhido
4. **Verifique** que o tomador foi excluído (status 204)
5. **Confirme** que o tomador não existe mais (GET retorna 404)

---

**Pronto!** Agora você sabe como excluir tomadores no Postman! 🚀

⚠️ **Lembre-se:** A exclusão é permanente. Use com cuidado!

