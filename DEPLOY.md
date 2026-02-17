# Deploy NFSe 2026 – VPS Linux (caprindweb.com.br)

Domínio único: **caprindweb.com.br** — Web na raiz e API em `/api`.

---

## Deploy com Git (recomendado)

No VPS você clona o repositório uma vez, configura Nginx e systemd, e a cada atualização faz `git pull` + script de deploy.

### Primeira vez no VPS

1. **Instalar .NET 8 SDK** (necessário para build no servidor):

   ```bash
   sudo apt update
   sudo apt install -y dotnet-sdk-8.0
   dotnet --version
   ```

2. **Clonar o repositório** (ajuste a URL do seu Git):

   ```bash
   sudo mkdir -p /var/nfse
   cd /var/nfse
   sudo git clone https://github.com/caprind/CaprindWebNFSe.git app
   cd app
   sudo chown -R $USER:$USER /var/nfse/app
   ```

3. **Criar diretórios de deploy e configurar produção** (antes do primeiro deploy):

   ```bash
   sudo mkdir -p /var/nfse/api /var/nfse/web
   ```

   Edite no servidor (após o primeiro deploy) e preencha uma vez:
   - `/var/nfse/api/appsettings.Production.json` — `ConnectionStrings:DefaultConnection` e `Jwt:Key`.

4. **Configurar Nginx e systemd** conforme as seções 5 e 6 abaixo.

5. **Primeiro deploy** (rodar na raiz do repositório `/var/nfse/app`):

   ```bash
   chmod +x scripts/deploy-vps.sh
   ./scripts/deploy-vps.sh
   ```

   Depois edite `/var/nfse/api/appsettings.Production.json` no servidor com a connection string e a Jwt:Key, e reinicie:  
   `sudo systemctl restart nfse-api`.

### A cada novo deploy

No PC, após commitar e dar push:

```bash
# No VPS, na pasta do repositório
cd /var/nfse/app
git pull
./scripts/deploy-vps.sh
```

O script publica API e Web, copia para `/var/nfse/api` e `/var/nfse/web`, preserva os `appsettings.Production.json` já configurados no servidor e reinicia os serviços.

---

## 1. Publicar no PC (PowerShell) – deploy manual

Na pasta da solution:

```powershell
dotnet publish NFSe2026.API/NFSe2026.API.csproj -c Release -o ./publish-api
dotnet publish NFSe2026.Web/NFSe2026.Web.csproj -c Release -o ./publish-web
```

As pastas `publish-api` e `publish-web` serão geradas na raiz do projeto.

---

## 2. Enviar para o VPS

```powershell
scp -r ./publish-api usuario@IP_DO_VPS:/var/nfse/api
scp -r ./publish-web usuario@IP_DO_VPS:/var/nfse/web
```

Se as pastas não existirem no VPS, crie antes por SSH: `mkdir -p /var/nfse/api /var/nfse/web`

---

## 3. No VPS – configurar produção

### API (`/var/nfse/api`)

- Editar `appsettings.Production.json` e preencher:
  - **ConnectionStrings:DefaultConnection** — string de conexão MySQL do servidor.
  - **Jwt:Key** — chave secreta com no mínimo 32 caracteres.
- Ou definir variáveis de ambiente (ex.: `ConnectionStrings__DefaultConnection`, `Jwt__Key`).

### Web (`/var/nfse/web`)

- Já está configurada com `ApiBaseUrl`: `https://caprindweb.com.br/api` em `appsettings.Production.json`.
- Se precisar alterar o domínio, edite esse arquivo no servidor.

---

## 4. Instalar .NET 8 no VPS (Ubuntu/Debian)

- **Deploy com Git:** instale o **SDK**: `sudo apt install -y dotnet-sdk-8.0` (já indicado na seção "Deploy com Git").
- **Deploy manual** (só publicar e copiar pastas): instale só o **runtime**:

```bash
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0
dotnet --list-runtimes
```

---

## 5. Nginx (Opção B – API em /api)

Arquivo: `/etc/nginx/sites-available/nfse`

```nginx
server {
    listen 80;
    server_name www.caprindweb.com.br caprindweb.com.br;

    location /api/ {
        proxy_pass http://127.0.0.1:5215/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location / {
        proxy_pass http://127.0.0.1:5103;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Ativar e testar:

```bash
sudo ln -s /etc/nginx/sites-available/nfse /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

SSL (Let's Encrypt):

```bash
sudo certbot --nginx -d caprindweb.com.br -d www.caprindweb.com.br
```

---

## 6. Systemd – API e Web como serviço

### API: `/etc/systemd/system/nfse-api.service`

```ini
[Unit]
Description=NFSe 2026 API
After=network.target

[Service]
WorkingDirectory=/var/nfse/api
ExecStart=/usr/bin/dotnet /var/nfse/api/NFSe2026.API.dll
Restart=always
RestartSec=5
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5215

[Install]
WantedBy=multi-user.target
```

### Web: `/etc/systemd/system/nfse-web.service`

```ini
[Unit]
Description=NFSe 2026 Web
After=network.target

[Service]
WorkingDirectory=/var/nfse/web
ExecStart=/usr/bin/dotnet /var/nfse/web/NFSe2026.Web.dll
Restart=always
RestartSec=5
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5103

[Install]
WantedBy=multi-user.target
```

Comandos:

```bash
sudo systemctl daemon-reload
sudo systemctl enable nfse-api nfse-web
sudo systemctl start nfse-api nfse-web
sudo systemctl status nfse-api nfse-web
```

---

## 7. URLs finais

| Uso        | URL                              |
|-----------|-----------------------------------|
| Site (Web)| https://caprindweb.com.br         |
| API       | https://caprindweb.com.br/api     |
| Swagger   | https://caprindweb.com.br/api/swagger |
