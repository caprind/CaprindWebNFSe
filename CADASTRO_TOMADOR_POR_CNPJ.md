# 📋 Guia: Cadastrar Tomador Apenas com CNPJ

## ✅ Novo Endpoint Criado

Foi criado um novo endpoint que permite cadastrar um tomador informando apenas o CNPJ. Os dados são buscados automaticamente da **Brasil API**.

## 🔗 Endpoint

- **URL**: `POST http://localhost:5215/api/tomador/por-cnpj`
- **Autenticação**: Obrigatória (JWT Bearer Token)
- **Content-Type**: `application/json`

## 📝 Como Usar no Postman

### 1. Pré-requisito: Estar Autenticado

Você precisa ter um token JWT válido. Faça login primeiro:
- `POST /api/auth/login`

### 2. Configuração no Postman

#### Passo 1: Criar a Requisição
1. Clique em "New" → "HTTP Request"
2. Ou use o botão "+" para nova aba

#### Passo 2: Configurar Método e URL
1. Selecione `POST` no dropdown
2. Digite: `http://localhost:5215/api/tomador/por-cnpj`

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
4. Cole o JSON:

```json
{
  "cnpj": "11222333000181"
}
```

### 3. Exemplo de Requisição Completa

**URL:**
```
POST http://localhost:5215/api/tomador/por-cnpj
```

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

**Body (raw JSON):**
```json
{
  "cnpj": "11222333000181"
}
```

**Ou com formatação:**
```json
{
  "cnpj": "11.222.333/0001-81"
}
```

> **Nota:** O CNPJ pode ser informado com ou sem formatação. O sistema remove automaticamente pontos, barras e hífens.

### 4. Resposta Esperada (201 Created)

```json
{
  "id": 1,
  "tipoPessoa": 2,
  "cpfcnpj": "11222333000181",
  "razaoSocialNome": "EMPRESA EXEMPLO LTDA",
  "inscricaoEstadual": "123456789",
  "inscricaoMunicipal": "987654321",
  "endereco": "Rua Exemplo",
  "numero": "123",
  "complemento": "Sala 10",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "uf": "SP",
  "cep": "01234567",
  "email": "contato@empresa.com.br",
  "telefone": "(11) 3456-7890"
}
```

### 5. Dados Preenchidos Automaticamente

O endpoint busca e preenche automaticamente:

- ✅ **TipoPessoa**: Sempre `Juridica` (2) para CNPJ
- ✅ **CPFCNPJ**: CNPJ informado (sem formatação)
- ✅ **RazaoSocialNome**: Razão Social da empresa
- ✅ **InscricaoEstadual**: Inscrição Estadual (se disponível)
- ✅ **InscricaoMunicipal**: Inscrição Municipal (se disponível)
- ✅ **Endereco**: Logradouro
- ✅ **Numero**: Número do endereço (ou "S/N" se não houver)
- ✅ **Complemento**: Complemento (se disponível)
- ✅ **Bairro**: Bairro
- ✅ **Cidade**: Cidade/Município
- ✅ **UF**: Estado (2 letras)
- ✅ **CEP**: CEP (sem formatação)
- ✅ **Email**: Email (se disponível na API)
- ✅ **Telefone**: Telefone (se disponível na API)

### 6. Possíveis Erros

#### Erro 400 - Bad Request

**CNPJ inválido:**
```json
{
  "error": "CNPJ inválido. Deve conter 14 dígitos."
}
```

**CNPJ não encontrado:**
```json
{
  "error": "Não foi possível consultar os dados do CNPJ. Verifique se o CNPJ está correto."
}
```

**Dados insuficientes:**
```json
{
  "error": "Não foi possível obter a razão social do CNPJ."
}
```

ou

```json
{
  "error": "Não foi possível obter o endereço do CNPJ."
}
```

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

#### Erro 409 - Conflict

**Tomador já existe:**
```json
{
  "error": "Já existe um tomador cadastrado com este CNPJ."
}
```

**Solução:**
- Use o endpoint `GET /api/tomador` para listar tomadores existentes
- Ou use `PUT /api/tomador/{id}` para atualizar o tomador existente

#### Erro 500 - Internal Server Error

```json
{
  "error": "Erro interno ao processar a solicitação."
}
```

**Causa:**
- Problema no servidor/banco de dados ou na API externa

**Solução:**
- Verifique se a aplicação está rodando
- Verifique os logs do servidor
- Verifique se a Brasil API está acessível

### 7. Fluxo Completo: Login → Cadastrar Tomador por CNPJ

#### Passo 1: Fazer Login

```
POST http://localhost:5215/api/auth/login

Headers:
Content-Type: application/json

Body:
{
  "email": "joao@empresa.com.br",
  "senha": "MinhaSenha123!"
}
```

Copie o `token` da resposta.

#### Passo 2: Cadastrar Tomador por CNPJ

```
POST http://localhost:5215/api/tomador/por-cnpj

Headers:
Authorization: Bearer {cole_o_token_aqui}
Content-Type: application/json

Body:
{
  "cnpj": "11222333000181"
}
```

### 8. Exemplos Práticos

#### Exemplo 1: CNPJ com Formatação

```json
POST http://localhost:5215/api/tomador/por-cnpj

Headers:
Authorization: Bearer eyJhbGci...
Content-Type: application/json

Body:
{
  "cnpj": "11.222.333/0001-81"
}
```

#### Exemplo 2: CNPJ sem Formatação

```json
POST http://localhost:5215/api/tomador/por-cnpj

Headers:
Authorization: Bearer eyJhbGci...
Content-Type: application/json

Body:
{
  "cnpj": "11222333000181"
}
```

### 9. Comparação: Endpoints Disponíveis

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| `/api/tomador` | POST | Cadastro completo manual (todos os campos) |
| `/api/tomador/por-cnpj` | POST | Cadastro automático apenas com CNPJ |
| `/api/tomador` | GET | Listar todos os tomadores |
| `/api/tomador/{id}` | GET | Obter tomador por ID |
| `/api/tomador/{id}` | PUT | Atualizar tomador |
| `/api/tomador/{id}` | DELETE | Excluir tomador |

### 10. Vantagens do Novo Endpoint

✅ **Mais Rápido**: Apenas informa o CNPJ  
✅ **Menos Erros**: Dados vêm direto da fonte oficial  
✅ **Mais Completo**: Busca todos os dados disponíveis  
✅ **Atualizado**: Dados sempre atualizados da Receita Federal  

### 11. Dicas

- O CNPJ pode ser informado com ou sem formatação
- Se o número do endereço não estiver disponível, será preenchido como "S/N"
- Alguns campos podem ser `null` se não estiverem disponíveis na API (ex: email, telefone)
- O sistema verifica se já existe um tomador com o mesmo CNPJ antes de criar

### 12. Teste Rápido

1. **Faça login** e copie o token
2. **Crie uma requisição POST** para `/api/tomador/por-cnpj`
3. **Adicione o header** `Authorization: Bearer {token}`
4. **Envie o body** com apenas o CNPJ:
   ```json
   {
     "cnpj": "11222333000181"
   }
   ```
5. **Veja o tomador criado** com todos os dados preenchidos automaticamente!

---

**Pronto!** Agora você pode cadastrar tomadores de forma muito mais rápida e simples! 🚀

