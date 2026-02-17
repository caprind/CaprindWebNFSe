
codigo de tributação nacional
010701

NBS: 111032200

# Plano do Projeto - Sistema de Emissão de NFS-e

## 1. Visão Geral
Sistema ASP.NET Core para emissão de Nota Fiscal de Serviços Eletrônica (NFS-e) integrado com a API Nacional, utilizando MySQL como banco de dados.

## 2. Estrutura do Projeto

```
NFSe2026/
├── NFSe2026.API/              # API principal
│   ├── Controllers/           # Endpoints da API
│   ├── Models/                # Modelos de domínio
│   ├── Services/              # Lógica de negócio e integração
│   ├── Data/                  # Contexto e configurações do EF Core
│   ├── DTOs/                  # Data Transfer Objects
│   ├── Configurations/        # Configurações (API Nacional, etc.)
│   └── Middlewares/           # Middlewares customizados
├── NFSe2026.Domain/           # Camada de domínio (opcional para DDD)
├── NFSe2026.Infrastructure/   # Camada de infraestrutura (opcional)
└── NFSe2026.Tests/            # Testes unitários
```

## 3. Tecnologias e Dependências

### Core
- ASP.NET Core 8.0 (ou 7.0)
- Entity Framework Core
- Pomelo.EntityFrameworkCore.MySql (provedor MySQL)

### Integração
- HttpClient para comunicação com API Nacional
- System.Text.Json para serialização

### Documentação
- Swagger/OpenAPI

### Outras
- AutoMapper (opcional, para mapeamento DTO ↔ Entity)
- FluentValidation (validação de dados)
- Serilog (logging)

## 4. Modelagem do Banco de Dados

### Entidades Principais

#### Prestador
- Id (int, PK)
- RazaoSocial (string)
- NomeFantasia (string)
- CNPJ (string)
- InscricaoMunicipal (string)
- Endereco (string)
- Cidade (string)
- UF (string)
- CEP (string)
- Telefone (string)
- Email (string)
- CertificadoDigital (string) - Base64 ou caminho
- SenhaCertificado (string) - criptografada
- Ambiente (enum: Homologacao, Producao)
- Ativo (bool)
- DataCriacao (DateTime)
- DataAtualizacao (DateTime)

#### Tomador
- Id (int, PK)
- TipoPessoa (enum: Fisica, Juridica)
- CPFCNPJ (string)
- RazaoSocialNome (string)
- InscricaoEstadual (string, nullable)
- InscricaoMunicipal (string, nullable)
- Endereco (string)
- Numero (string)
- Complemento (string, nullable)
- Bairro (string)
- Cidade (string)
- UF (string)
- CEP (string)
- Email (string, nullable)
- Telefone (string, nullable)
- DataCriacao (DateTime)
- DataAtualizacao (DateTime)

#### NotaFiscal
- Id (int, PK)
- PrestadorId (int, FK)
- TomadorId (int, FK)
- Numero (string) - Número gerado pela API
- CodigoVerificacao (string)
- Serie (string)
- Competencia (DateTime)
- DataEmissao (DateTime)
- DataVencimento (DateTime, nullable)
- ValorServicos (decimal)
- ValorDeducoes (decimal, default 0)
- ValorPis (decimal, default 0)
- ValorCofins (decimal, default 0)
- ValorInss (decimal, default 0)
- ValorIr (decimal, default 0)
- ValorCsll (decimal, default 0)
- ValorIss (decimal, default 0)
- ValorIssRetido (decimal, default 0)
- ValorLiquido (decimal)
- Situacao (enum: Rascunho, Autorizada, Cancelada, Rejeitada)
- DiscriminacaoServicos (string)
- CodigoMunicipio (string)
- Observacoes (string, nullable)
- XML (text) - XML da nota
- JSON (text) - JSON da nota
- DataCriacao (DateTime)
- DataAtualizacao (DateTime)

#### ItemServico
- Id (int, PK)
- NotaFiscalId (int, FK)
- CodigoServico (string) - Código da lista de serviços
- Discriminacao (string)
- Quantidade (decimal)
- ValorUnitario (decimal)
- ValorTotal (decimal)
- AliquotaIss (decimal)
- ItemListaServico (string)

#### ConfiguracaoAPI
- Id (int, PK)
- Ambiente (enum: Homologacao, Producao)
- UrlBase (string)
- ClientId (string)
- ClientSecret (string)
- Scope (string)
- Timeout (int) - em segundos
- DataCriacao (DateTime)
- DataAtualizacao (DateTime)

## 5. Endpoints da API

### Prestador
- GET /api/prestador - Listar prestadores
- GET /api/prestador/{id} - Obter prestador por ID
- POST /api/prestador - Criar prestador
- PUT /api/prestador/{id} - Atualizar prestador
- DELETE /api/prestador/{id} - Deletar prestador

### Tomador
- GET /api/tomador - Listar tomadores
- GET /api/tomador/{id} - Obter tomador por ID
- POST /api/tomador - Criar tomador
- PUT /api/tomador/{id} - Atualizar tomador
- DELETE /api/tomador/{id} - Deletar tomador

### Nota Fiscal
- GET /api/notafiscal - Listar notas fiscais
- GET /api/notafiscal/{id} - Obter nota por ID
- POST /api/notafiscal - Emitir nova nota fiscal
- POST /api/notafiscal/{id}/cancelar - Cancelar nota fiscal
- GET /api/notafiscal/{id}/consultar - Consultar situação na API
- GET /api/notafiscal/{id}/xml - Download do XML
- GET /api/notafiscal/{id}/pdf - Download do PDF (se disponível)

## 6. Serviços de Integração

### NFSeAPIService
- Autenticar()
- EmitirNotaFiscal(NotaFiscalDTO)
- CancelarNotaFiscal(string numero, string codigoVerificacao, string motivo)
- ConsultarNotaFiscal(string numero, string codigoVerificacao)
- GerarToken() - OAuth/JWT conforme API Nacional

### NotaFiscalService
- ValidarDados(NotaFiscalDTO)
- CalcularValores(NotaFiscal)
- Emitir(NotaFiscalDTO)
- Cancelar(int notaFiscalId, string motivo)
- ConsultarSituacao(int notaFiscalId)

## 7. Configurações

### appsettings.json
- ConnectionString (MySQL)
- ApiNacionalNFSe (URL, credenciais, ambiente)
- JWT settings (se necessário)
- Logging

## 8. Fluxo de Emissão de NFS-e

1. Usuário cria/atualiza dados do prestador
2. Usuário cria/atualiza dados do tomador
3. Usuário preenche dados da nota fiscal (serviços, valores, etc.)
4. Sistema valida os dados
5. Sistema autentica na API Nacional (obtém token)
6. Sistema envia requisição para emissão
7. API Nacional retorna resposta (sucesso ou erro)
8. Sistema salva XML/JSON da nota no banco
9. Sistema atualiza status da nota
10. Sistema retorna resultado para o usuário

## 9. Segurança

- HTTPS obrigatório
- Autenticação e autorização (JWT)
- Criptografia de dados sensíveis (certificado, senhas)
- Validação de entrada em todos os endpoints
- Rate limiting (se necessário)

## 10. Logging e Monitoramento

- Log de todas as operações com API Nacional
- Log de erros e exceções
- Log de auditoria (criação, alteração, cancelamento de notas)
- Health checks

## 11. Próximos Passos

1. Criar estrutura do projeto
2. Configurar banco de dados MySQL
3. Criar models e DbContext
4. Criar migrations
5. Implementar serviços de integração
6. Criar controllers
7. Configurar Swagger
8. Implementar validações
9. Adicionar logging
10. Testes

