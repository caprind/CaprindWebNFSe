# ✏️ Guia: Atualizar/Alterar Tomador

## ✅ Endpoint de Atualização

- **URL**: `PUT http://localhost:5215/api/tomador/{id}`
- **Autenticação**: Obrigatória (JWT Bearer Token)
- **Content-Type**: `application/json`

## 📝 Como Usar no Postman

### 1. Pré-requisito: Estar Autenticado

Você precisa ter um token JWT válido. Faça login primeiro:
- `POST /api/auth/login`

### 2. Descobrir o ID do Tomador

Antes de atualizar, você precisa saber o ID do tomador. Use um dos métodos:

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
1. Selecione `PUT` no dropdown
2. Digite: `http://localhost:5215/api/tomador/{id}`
   - Substitua `{id}` pelo ID do tomador que deseja atualizar
   - Exemplo: `http://localhost:5215/api/tomador/1`

#### Passo 3: Configurar Autenticação
Na aba **"Authorization"**:
- Type: `Bearer Token`
- Token: cole o token JWT obtido no login

OU na aba **"Headers"**, adicione:
- Key: `Authorization`
- Value: `Bearer {seu_token_aqui}`

#### Passo 4: Configurar Headers
1. Vá na aba **"Headers"**
2. Adicione:
   - Key: `Content-Type`
   - Value: `application/json`

#### Passo 5: Configurar Body
1. Vá na aba **"Body"**
2. Selecione `raw`
3. No dropdown à direita, selecione `JSON`
4. Cole o JSON com os campos que deseja atualizar (veja exemplos abaixo)

### 4. Exemplos de Body

#### Exemplo 1: Atualizar Campos Completos (Pessoa Jurídica)

```json
{
  "tipoPessoa": 2,
  "razaoSocialNome": "EMPRESA ATUALIZADA LTDA",
  "inscricaoEstadual": "987654321",
  "inscricaoMunicipal": "123456789",
  "endereco": "Avenida Atualizada",
  "numero": "200",
  "complemento": "Sala 50",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "uf": "SP",
  "cep": "01310100",
  "email": "novoemail@empresa.com.br",
  "telefone": "(11) 98765-4321"
}
```

#### Exemplo 2: Atualizar Apenas Alguns Campos

```json
{
  "tipoPessoa": 2,
  "razaoSocialNome": "EMPRESA EXEMPLO LTDA",
  "endereco": "Rua Exemplo",
  "numero": "123",
  "bairro": "Centro",
  "cidade": "Rio de Janeiro",
  "uf": "RJ",
  "cep": "20000000"
}
```

#### Exemplo 3: Atualizar Pessoa Física

```json
{
  "tipoPessoa": 1,
  "razaoSocialNome": "João da Silva Santos",
  "endereco": "Rua Nova",
  "numero": "456",
  "complemento": "Apto 10",
  "bairro": "Copacabana",
  "cidade": "Rio de Janeiro",
  "uf": "RJ",
  "cep": "22010000",
  "email": "joao.santos@email.com",
  "telefone": "(21) 98765-4321"
}
```

### 5. Campos do Body

#### Campos Obrigatórios:
- `tipoPessoa` (int): `1` = Pessoa Física, `2` = Pessoa Jurídica
- `razaoSocialNome` (string): Nome completo (PF) ou Razão Social (PJ)
- `endereco` (string): Logradouro
- `numero` (string): Número do endereço
- `bairro` (string): Bairro
- `cidade` (string): Cidade
- `uf` (string): UF (2 letras, ex: "SP", "RJ")
- `cep` (string): CEP (8 dígitos, sem formatação)

#### Campos Opcionais:
- `inscricaoEstadual` (string): IE (geralmente para PJ)
- `inscricaoMunicipal` (string): IM (geralmente para PJ)
- `complemento` (string): Complemento do endereço
- `email` (string): Email de contato
- `telefone` (string): Telefone de contato

**⚠️ Importante:** 
- O campo `CPFCNPJ` **NÃO** pode ser alterado (não está no DTO de atualização)
- Se você não informar um campo obrigatório, ele será atualizado com o valor vazio/fornecido

### 6. Resposta Esperada (204 No Content)

Quando a atualização for bem-sucedida, a resposta será:
- **Status Code**: `204 No Content`
- **Body**: Vazio (sem conteúdo)

Para verificar se a atualização funcionou, busque o tomador novamente:

```
GET http://localhost:5215/api/tomador/{id}
```

### 7. Possíveis Erros

#### Erro 400 - Bad Request

**Validação de campos:**
```json
{
  "errors": {
    "razaoSocialNome": ["O campo RazaoSocialNome é obrigatório."],
    "uf": ["O campo UF deve ter 2 caracteres."]
  }
}
```

**Causas:**
- Campos obrigatórios faltando
- Enum `tipoPessoa` inválido (deve ser 1 ou 2)
- Formato de dados inválido (ex: UF com mais de 2 caracteres)

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

**Solução:**
- Verifique se a aplicação está rodando
- Verifique os logs do servidor

### 8. Fluxo Completo: Buscar → Atualizar → Verificar

#### Passo 1: Buscar o Tomador

```
GET http://localhost:5215/api/tomador/1

Headers:
Authorization: Bearer {seu_token}
```

Resposta:
```json
{
  "id": 1,
  "tipoPessoa": 2,
  "cpfcnpj": "11222333000181",
  "razaoSocialNome": "EMPRESA EXEMPLO LTDA",
  "endereco": "Rua Antiga",
  "numero": "100",
  ...
}
```

#### Passo 2: Atualizar o Tomador

```
PUT http://localhost:5215/api/tomador/1

Headers:
Authorization: Bearer {seu_token}
Content-Type: application/json

Body:
{
  "tipoPessoa": 2,
  "razaoSocialNome": "EMPRESA EXEMPLO LTDA",
  "endereco": "Rua Nova Atualizada",
  "numero": "200",
  "complemento": "Sala 10",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "uf": "SP",
  "cep": "01310100",
  "email": "novoemail@empresa.com.br",
  "telefone": "(11) 98765-4321"
}
```

Resposta: `204 No Content`

#### Passo 3: Verificar a Atualização

```
GET http://localhost:5215/api/tomador/1

Headers:
Authorization: Bearer {seu_token}
```

Agora você verá os dados atualizados!

### 9. Exemplo Prático Completo

#### Atualizar Endereço e Contato:

```json
PUT http://localhost:5215/api/tomador/1

Headers:
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

Body:
{
  "tipoPessoa": 2,
  "razaoSocialNome": "EMPRESA CLIENTE LTDA",
  "inscricaoEstadual": "123456789",
  "endereco": "Rua Nova Endereço",
  "numero": "500",
  "complemento": "Andar 3",
  "bairro": "Jardim Paulista",
  "cidade": "São Paulo",
  "uf": "SP",
  "cep": "01415000",
  "email": "contato@cliente.com.br",
  "telefone": "(11) 3456-7890"
}
```

### 10. Valores do Enum TipoPessoa

- `1` = Fisica (Pessoa Física)
- `2` = Juridica (Pessoa Jurídica)

### 11. Dicas Importantes

✅ **O que você PODE alterar:**
- Nome/Razão Social
- Endereço completo
- Inscrições (IE e IM)
- Email e Telefone
- Tipo de Pessoa

❌ **O que você NÃO PODE alterar:**
- CPF/CNPJ (campo não está disponível no DTO de atualização)
- ID (é a chave primária)

📝 **Observações:**
- Você deve informar TODOS os campos obrigatórios, mesmo que não esteja alterando
- Campos opcionais podem ser omitidos ou enviados como `null`
- O campo `dataAtualizacao` é atualizado automaticamente pelo sistema

### 12. Comparação: Endpoints Disponíveis

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/tomador` | GET | Listar todos os tomadores |
| `/api/tomador/{id}` | GET | Obter tomador por ID |
| `/api/tomador` | POST | Cadastrar tomador (manual completo) |
| `/api/tomador/por-cnpj` | POST | Cadastrar tomador apenas com CNPJ |
| `/api/tomador/{id}` | PUT | **Atualizar tomador** |
| `/api/tomador/{id}` | DELETE | Excluir tomador |

---

**Pronto!** Agora você sabe como atualizar tomadores no Postman! 🚀

