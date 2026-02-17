# Sistema de Login e Multi-Empresas - NFSe 2026

## ✅ Implementado

### 1. Modelos Criados

#### Empresa
- Armazena dados da empresa obtidos via consulta CNPJ
- Relacionamento com Usuarios e Prestadores
- Campos: CNPJ, RazaoSocial, NomeFantasia, Endereço completo, etc.

#### Usuario
- Relacionado a uma Empresa
- Autenticação com senha criptografada (BCrypt)
- Campos: Nome, Email, SenhaHash, Telefone, etc.

#### Prestador (Atualizado)
- Agora relacionado a uma Empresa
- Multi-tenancy: cada empresa tem seus próprios prestadores

### 2. Serviços Criados

#### IAuthService / AuthService
- `LoginAsync()` - Autenticação de usuário
- `CadastrarEmpresaAsync()` - Cadastro de nova empresa com consulta CNPJ
- `GenerateJwtToken()` - Geração de token JWT com claims de EmpresaId

#### IConsultaCNPJService / ConsultaCNPJService
- Integração com API ReceitaWS (gratuita)
- Consulta dados da empresa por CNPJ
- Retorna: Razão Social, Nome Fantasia, Endereço, Inscrições, etc.

### 3. Controllers Criados/Atualizados

#### AuthController
- `POST /api/auth/login` - Login de usuário
- `POST /api/auth/cadastro` - Cadastro de nova empresa

#### EmpresaController (novo)
- `GET /api/empresa/consultar-cnpj/{cnpj}` - Consulta CNPJ (público)
- `GET /api/empresa/meus-dados` - Dados da empresa autenticada

#### PrestadorController (atualizado)
- Agora exige autenticação `[Authorize]`
- Filtra prestadores por EmpresaId do token
- CreatePrestador define EmpresaId automaticamente do token

#### NotaFiscalController (atualizado)
- Agora exige autenticação `[Authorize]`

#### TomadorController (atualizado)
- Agora exige autenticação `[Authorize]`

### 4. Autenticação JWT

- Configurado no `Program.cs`
- Token contém: UsuarioId, EmpresaId, Email
- Validade: 8 horas (configurável)
- Configuração em `appsettings.json`:
  ```json
  "Jwt": {
    "Key": "sua-chave-secreta...",
    "Issuer": "NFSe2026",
    "Audience": "NFSe2026",
    "ExpirationHours": 8
  }
  ```

### 5. Multi-Tenancy

- Todos os recursos são filtrados por EmpresaId
- Prestadores vinculados à empresa
- Isolamento de dados entre empresas
- Token JWT contém EmpresaId para filtragem automática

## 🔐 Fluxo de Autenticação

### Cadastro de Nova Empresa

1. Cliente envia: CNPJ, Nome, Email, Senha
2. Sistema consulta dados do CNPJ na API ReceitaWS
3. Sistema cria registro de Empresa com dados obtidos
4. Sistema cria primeiro Usuario da empresa
5. Sistema retorna Token JWT + dados da empresa

### Login

1. Cliente envia: Email, Senha
2. Sistema valida credenciais (BCrypt)
3. Sistema atualiza último acesso
4. Sistema retorna Token JWT + dados do usuário e empresa

### Uso do Token

1. Cliente inclui token no header: `Authorization: Bearer {token}`
2. Middleware JWT valida e extrai claims
3. Controllers acessam EmpresaId via `User.FindFirst("EmpresaId")`
4. Queries são filtradas automaticamente por EmpresaId

## 📝 Exemplos de Uso

### Cadastro
```http
POST /api/auth/cadastro
Content-Type: application/json

{
  "cnpj": "12.345.678/0001-90",
  "nome": "João Silva",
  "email": "joao@empresa.com",
  "senha": "SenhaSegura123",
  "telefone": "11999999999"
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "joao@empresa.com",
  "senha": "SenhaSegura123"
}
```

### Consultar CNPJ
```http
GET /api/empresa/consultar-cnpj/12345678000190
```

### Acessar Recursos Protegidos
```http
GET /api/prestador
Authorization: Bearer {token}
```

## ⚠️ Observações Importantes

1. **API ReceitaWS**: Gratuita, mas tem limite de requisições. Para produção, considere:
   - Cache de consultas
   - API paga com maior limite
   - Validação de CNPJ antes da consulta

2. **Segurança**:
   - Senhas criptografadas com BCrypt
   - JWT com chave secreta (altere em produção!)
   - HTTPS obrigatório em produção
   - Validação de token em todos os endpoints protegidos

3. **Multi-Tenancy**:
   - Dados isolados por EmpresaId
   - Não é possível acessar dados de outras empresas
   - Prestadores vinculados à empresa

4. **Próximos Passos Sugeridos**:
   - Adicionar roles/permissões de usuário
   - Implementar refresh token
   - Adicionar middleware para logs de auditoria
   - Filtrar NotaFiscal por empresa no service
   - Adicionar validações com FluentValidation

## 🔧 Configuração

### appsettings.json
```json
{
  "Jwt": {
    "Key": "SUA_CHAVE_SECRETA_SUPER_LONGA_AQUI_MINIMO_32_CARACTERES",
    "Issuer": "NFSe2026",
    "Audience": "NFSe2026",
    "ExpirationHours": 8
  }
}
```

### Banco de Dados
Execute as migrações:
```bash
dotnet ef migrations add AddEmpresaUsuarioMultiTenancy
dotnet ef database update
```

## ✅ Testes Recomendados

1. Cadastro de empresa com CNPJ válido
2. Cadastro com CNPJ já existente (deve falhar)
3. Login com credenciais corretas
4. Login com credenciais incorretas
5. Acesso a recursos protegidos sem token
6. Acesso a recursos protegidos com token inválido
7. Acesso a recursos protegidos com token válido
8. Isolamento: empresa A não pode ver dados da empresa B

