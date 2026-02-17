# ✅ Remoção da Tabela Prestadores - Empresa é o Prestador

## 📋 Resumo

Foi implementada a remoção da tabela `Prestadores`, pois **a empresa logada é o prestador de serviços**. Todas as notas fiscais agora se relacionam diretamente com a `Empresa`.

## 🔧 Mudanças Implementadas

### 1. Modelo Empresa
- ✅ Adicionado `CertificadoDigital` (StringLength 5000)
- ✅ Adicionado `SenhaCertificado` (StringLength 500)
- ✅ Adicionado `Ambiente` (enum Ambiente, default Homologacao)
- ✅ Adicionado relacionamento com `NotasFiscais`

### 2. Modelo NotaFiscal
- ✅ Removido `PrestadorId`
- ✅ Adicionado `EmpresaId` (obrigatório)
- ✅ Relacionamento alterado de `Prestador` para `Empresa`

### 3. ApplicationDbContext
- ✅ Removido `DbSet<Prestador> Prestadores`
- ✅ Removida configuração de `Prestador`
- ✅ Atualizada configuração de `NotaFiscal` para usar `EmpresaId`

### 4. DTOs
- ✅ `NotaFiscalDTO`: `PrestadorId` → `EmpresaId`
- ✅ `NotaFiscalCreateDTO`: Removido `PrestadorId` (obtido do token JWT)

### 5. Services
- ✅ `INotaFiscalService`: Atualizado para usar `empresaId` em todos os métodos
- ✅ `NotaFiscalService`: Filtra por `EmpresaId` diretamente
- ✅ `INFSeAPIService`: `prestadorId` → `empresaId` em todos os métodos
- ✅ `NFSeAPIService`: Usa `Empresa` em vez de `Prestador` para obter certificado e ambiente

### 6. Controllers
- ✅ `NotaFiscalController`: Remove parâmetro `prestadorId` do GET
- ✅ `PrestadorController`: **REMOVIDO** (não é mais necessário)

### 7. Mappings
- ✅ Removidos mappings de `Prestador`
- ✅ Atualizado mapping de `NotaFiscal` para usar `Empresa`

### 8. TestConnection
- ✅ Removida verificação da tabela `Prestadores`

## 📊 Nova Estrutura de Dados

### Relacionamentos

```
Empresa (Prestador de Serviços)
  ├── Usuarios (1:N)
  ├── Tomadores (1:N)
  └── NotasFiscais (1:N) ← NOVO

NotaFiscal
  ├── Empresa (N:1) → EmpresaId (empresa logada = prestador)
  └── Tomador (N:1) → TomadorId
```

### Campos Adicionados em Empresa

- `CertificadoDigital` (StringLength 5000): Certificado digital para assinatura
- `SenhaCertificado` (StringLength 500): Senha do certificado (criptografada)
- `Ambiente` (Ambiente enum): Ambiente de homologação ou produção

## 🗄️ Migration

Foi criada a migration `RemoverPrestadorEAtualizarNotaFiscal` que:
- ✅ Adiciona colunas `CertificadoDigital`, `SenhaCertificado`, `Ambiente` em `Empresas`
- ✅ Remove foreign key `FK_NotasFiscais_Prestadores_PrestadorId`
- ✅ Remove índice `IX_NotasFiscais_PrestadorId`
- ✅ Adiciona coluna `EmpresaId` em `NotasFiscais`
- ✅ Cria foreign key `FK_NotasFiscais_Empresas_EmpresaId`
- ✅ Cria índice `IX_NotasFiscais_EmpresaId`
- ✅ Remove tabela `Prestadores`

### ⚠️ Importante: Aplicação da Migration

**Se o banco de dados JÁ EXISTE com dados:**

A migration irá:
1. **Adicionar campos** em `Empresas` (CertificadoDigital, SenhaCertificado, Ambiente)
2. **Migrar dados** de `Prestadores` para `Empresas` (se necessário)
3. **Atualizar** `NotasFiscais` para usar `EmpresaId`
4. **Remover** a tabela `Prestadores`

**⚠️ ATENÇÃO:** Se houver dados em `Prestadores`, você precisará migrar manualmente antes de aplicar a migration:

```sql
-- 1. Adicionar campos em Empresas (se ainda não existirem)
ALTER TABLE Empresas 
ADD COLUMN CertificadoDigital VARCHAR(5000) NULL,
ADD COLUMN SenhaCertificado VARCHAR(500) NULL,
ADD COLUMN Ambiente INT NOT NULL DEFAULT 1;

-- 2. Migrar dados de Prestadores para Empresas (se necessário)
UPDATE Empresas e
INNER JOIN Prestadores p ON e.Id = p.EmpresaId
SET 
    e.CertificadoDigital = p.CertificadoDigital,
    e.SenhaCertificado = p.SenhaCertificado,
    e.Ambiente = p.Ambiente
WHERE p.Ativo = 1;

-- 3. Atualizar NotasFiscais para usar EmpresaId
UPDATE NotasFiscais nf
INNER JOIN Prestadores p ON nf.PrestadorId = p.Id
SET nf.EmpresaId = p.EmpresaId;

-- 4. Aplicar a migration normalmente
```

**Para aplicar a migration:**

```bash
dotnet ef database update --context ApplicationDbContext
```

## 🔐 Segurança

Todas as operações agora garantem que:
- ✅ Nota fiscal sempre pertence à empresa logada (prestador)
- ✅ Não é possível criar nota fiscal para outra empresa
- ✅ Validação em todas as operações CRUD

## 📝 Arquivos Modificados

- ✅ `NFSe2026.API/Models/Empresa.cs` - Adicionados campos de prestador
- ✅ `NFSe2026.API/Models/NotaFiscal.cs` - PrestadorId → EmpresaId
- ✅ `NFSe2026.API/Data/ApplicationDbContext.cs` - Removido Prestador
- ✅ `NFSe2026.API/DTOs/NotaFiscalDTO.cs` - PrestadorId → EmpresaId
- ✅ `NFSe2026.API/Services/INotaFiscalService.cs` - Atualizado
- ✅ `NFSe2026.API/Services/NotaFiscalService.cs` - Atualizado
- ✅ `NFSe2026.API/Services/INFSeAPIService.cs` - Atualizado
- ✅ `NFSe2026.API/Services/NFSeAPIService.cs` - Atualizado
- ✅ `NFSe2026.API/Controllers/NotaFiscalController.cs` - Atualizado
- ✅ `NFSe2026.API/Controllers/PrestadorController.cs` - **REMOVIDO**
- ✅ `NFSe2026.API/Mappings/MappingProfile.cs` - Removidos mappings de Prestador
- ✅ `NFSe2026.API/TestConnection.cs` - Removida verificação de Prestadores
- ✅ `NFSe2026.API/Migrations/20251228011727_RemoverPrestadorEAtualizarNotaFiscal.cs` (NOVA)

## 🧪 Como Testar

1. **Aplicar a migration**
   ```bash
   dotnet ef database update --context ApplicationDbContext
   ```

2. **Verificar estrutura**
   - Tabela `Prestadores` não deve mais existir
   - Tabela `NotasFiscais` deve ter coluna `EmpresaId`
   - Tabela `Empresas` deve ter `CertificadoDigital`, `SenhaCertificado`, `Ambiente`

3. **Testar criação de nota fiscal**
   - Login com empresa
   - Criar nota fiscal → deve usar empresa logada como prestador
   - Listar notas fiscais → deve mostrar apenas notas da empresa logada

## ✅ Status

- ✅ Modelos atualizados
- ✅ Controllers atualizados
- ✅ Services atualizados
- ✅ DTOs atualizados
- ✅ Mappings atualizados
- ✅ Migration criada
- ⚠️ **Migration precisa ser aplicada ao banco de dados**

---

**Implementação concluída!** O sistema agora usa a empresa logada como prestador de serviços! 🎉

