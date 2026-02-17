# 🔧 Solução: Erro ao Carregar Tomadores - Autenticação

## ❌ Problema

Ao tentar listar tomadores, aparece a mensagem:
```
Erro ao carregar tomadores. Verifique se você está autenticado.
```

## 🔍 Causa

O problema pode ter várias causas:

1. **Token JWT não está sendo enviado** na requisição
2. **Token JWT expirado ou inválido**
3. **EmpresaId não está no token** (claim faltando)
4. **Sessão expirada** no front-end
5. **Erro na API ao obter EmpresaId** do token

## ✅ Soluções Implementadas

### 1. Tratamento de Erros 401 na API

O `TomadorController.GetTomadores()` agora trata `UnauthorizedAccessException` e retorna HTTP 401 corretamente:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<TomadorDTO>>> GetTomadores()
{
    try
    {
        var empresaId = ObterEmpresaId();
        var tomadores = await _context.Tomadores
            .Where(t => t.EmpresaId == empresaId)
            .ToListAsync();
        return Ok(_mapper.Map<IEnumerable<TomadorDTO>>(tomadores));
    }
    catch (UnauthorizedAccessException)
    {
        return Unauthorized(new { error = "Empresa não identificada no token" });
    }
}
```

### 2. Melhor Tratamento no Front-End

O `ApiService.GetAsync()` agora detecta erro 401 e lança `UnauthorizedAccessException`:

```csharp
if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");
}
```

### 3. Redirecionamento Automático para Login

O `TomadorController.Index()` no Web agora:
- Verifica se há token na sessão
- Se não houver token, redireciona para login
- Se houver erro 401, captura e redireciona para login com mensagem

## 🧪 Como Diagnosticar

### 1. Verificar Logs da API

Verifique os logs da API para ver qual erro está sendo retornado:

```bash
# No console onde a API está rodando, procure por:
# "GET tomador retornou 401" ou similar
```

### 2. Verificar Token na Sessão (Front-End)

Abra o DevTools do navegador (F12) e verifique:
- **Application → Session Storage** (ou Local Storage)
- Procure por `JWTToken`
- Verifique se existe e se não está vazio

### 3. Verificar Token no Network

1. Abra DevTools (F12)
2. Vá para a aba **Network**
3. Tente carregar a lista de tomadores
4. Clique na requisição `GET /api/tomador`
5. Verifique:
   - **Headers → Authorization**: Deve conter `Bearer <token>`
   - **Status Code**: Se for 401, o token está inválido/expirado

### 4. Testar Token Manualmente

Você pode testar o token diretamente:

```bash
# Via Postman ou curl
GET http://localhost:5215/api/tomador
Headers:
  Authorization: Bearer <seu-token>
```

## 🔧 Soluções Rápidas

### Solução 1: Fazer Login Novamente

Se a sessão expirou:
1. **Faça logout**
2. **Faça login novamente**
3. Tente carregar os tomadores

### Solução 2: Verificar Se Está Logado

Verifique se você está realmente autenticado:
- Veja se seu nome aparece no canto superior direito
- Tente acessar outras páginas (Prestadores, Notas Fiscais)
- Se outras páginas também não funcionam, a sessão expirou

### Solução 3: Limpar Sessão

Se houver problemas persistentes:
1. **Limpe os cookies/sessão** do navegador
2. **Faça login novamente**
3. Tente novamente

### Solução 4: Verificar Migration

Se você acabou de aplicar a migration de multi-tenancy:
- Certifique-se de que a migration foi aplicada
- Verifique se a coluna `EmpresaId` existe na tabela `Tomadores`
- Verifique se há dados na tabela `Tomadores` com `EmpresaId` preenchido

## 📝 Verificações Adicionais

### Verificar Se a Migration Foi Aplicada

```sql
-- Verificar se a coluna EmpresaId existe
DESCRIBE Tomadores;

-- Verificar se há dados
SELECT COUNT(*) FROM Tomadores;

-- Verificar se há EmpresaId preenchido
SELECT COUNT(*) FROM Tomadores WHERE EmpresaId IS NOT NULL;
```

### Verificar Se Há Tomadores Para a Empresa Logada

```sql
-- Substitua 1 pelo ID da sua empresa logada
SELECT * FROM Tomadores WHERE EmpresaId = 1;
```

## 🎯 Comportamento Esperado

**Se estiver tudo funcionando:**
- ✅ A lista de tomadores carrega normalmente
- ✅ Se não houver tomadores, mostra "Nenhum tomador cadastrado"
- ✅ Se houver tomadores, mostra a lista

**Se houver problema de autenticação:**
- ✅ Redireciona automaticamente para a página de login
- ✅ Mostra mensagem "Sessão expirada. Faça login novamente."

**Se não houver token na sessão:**
- ✅ Redireciona automaticamente para a página de login

## 📊 Status da Implementação

- ✅ Tratamento de erro 401 na API
- ✅ Detecção de erro 401 no ApiService
- ✅ Redirecionamento automático para login
- ✅ Verificação de token na sessão
- ✅ Mensagens de erro melhoradas
- ✅ Logs melhorados para diagnóstico

---

**Problema resolvido!** O sistema agora trata adequadamente erros de autenticação e redireciona para login quando necessário! 🎉

