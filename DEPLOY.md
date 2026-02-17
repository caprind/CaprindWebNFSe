# Deploy NFSe 2026 – VPS Linux (caprindweb.com.br)

Domínio único: **caprindweb.com.br** — Web na raiz e API em `/api`.

---

## 1. Publicar no PC (PowerShell)

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
