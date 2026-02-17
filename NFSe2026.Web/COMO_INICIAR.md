# 🚀 Como Iniciar a Aplicação

## ⚠️ Importante

A aplicação **Web** depende da **API** estar rodando. Sem a API, você verá erros como:
```
Nenhuma conexão pôde ser feita porque a máquina de destino as recusou ativamente. (localhost:5215)
```

## 📋 Passos para Iniciar

### 1️⃣ Iniciar a API (Primeiro!)

Abra um terminal e execute:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run
```

Ou usando o perfil específico:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.API"
dotnet run --launch-profile http
```

A API deve iniciar na porta **5215** (HTTP) ou **7179** (HTTPS).

**Verifique se a API está rodando:** Acesse `http://localhost:5215` ou `http://localhost:5215/swagger` no navegador.

### 2️⃣ Iniciar a Web (Depois)

Abra **outro terminal** (mantenha a API rodando) e execute:

```powershell
cd "C:\Projetos IA\NFSe 2026\NFSe2026.Web"
dotnet run
```

A Web deve iniciar na porta **5103** (HTTP) ou **7296** (HTTPS).

**Acesse a aplicação:** `http://localhost:5103` ou `https://localhost:7296`

## 🔧 Configuração de Portas

### API (NFSe2026.API)
- **HTTP**: `http://localhost:5215`
- **HTTPS**: `https://localhost:7179`
- Configurado em: `NFSe2026.API/Properties/launchSettings.json`

### Web (NFSe2026.Web)
- **HTTP**: `http://localhost:5103`
- **HTTPS**: `https://localhost:7296`
- Configurado em: `NFSe2026.Web/Properties/launchSettings.json`

A Web está configurada para se conectar à API em: `http://localhost:5215` (configurado em `NFSe2026.Web/appsettings.json`)

## ⚠️ Solução de Problemas

### Erro: "Nenhuma conexão pôde ser feita"
1. Verifique se a API está rodando
2. Verifique se a porta 5215 está disponível
3. Tente acessar `http://localhost:5215/swagger` no navegador
4. Se a API não iniciar, verifique os logs para erros

### Mudar a porta da API
1. Edite `NFSe2026.API/Properties/launchSettings.json`
2. Altere `applicationUrl` no perfil desejado
3. Edite `NFSe2026.Web/appsettings.json`
4. Altere `ApiBaseUrl` para a nova porta

## 💡 Dica: Usar Visual Studio

No Visual Studio, você pode:
1. Configurar múltiplos projetos de inicialização
2. Clicar com botão direito na solução → Properties → Startup Project
3. Selecionar "Multiple startup projects"
4. Definir ambos (API e Web) como "Start"



