#!/bin/bash
# Deploy NFSe 2026 no VPS – executar na raiz do repositório (após git pull)
# Uso: ./scripts/deploy-vps.sh

set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_DEST="/var/nfse/api"
WEB_DEST="/var/nfse/web"

cd "$REPO_ROOT"

echo "[1/6] Publicando API..."
dotnet publish NFSe2026.API/NFSe2026.API.csproj -c Release -o "$REPO_ROOT/publish-api" --no-self-contained

echo "[2/6] Publicando Web..."
dotnet publish NFSe2026.Web/NFSe2026.Web.csproj -c Release -o "$REPO_ROOT/publish-web" --no-self-contained

echo "[3/6] Preservando appsettings.Production.json do servidor..."
[ -f "$API_DEST/appsettings.Production.json" ] && cp "$API_DEST/appsettings.Production.json" "$REPO_ROOT/appsettings.api.production.bak"
[ -f "$WEB_DEST/appsettings.Production.json" ] && cp "$WEB_DEST/appsettings.Production.json" "$REPO_ROOT/appsettings.web.production.bak"

echo "[4/6] Copiando arquivos para /var/nfse..."
sudo rm -rf "$API_DEST" "$WEB_DEST"
sudo mkdir -p "$API_DEST" "$WEB_DEST"
sudo cp -r "$REPO_ROOT/publish-api"/* "$API_DEST/"
sudo cp -r "$REPO_ROOT/publish-web"/* "$WEB_DEST/"
sudo chown -R www-data:www-data "$API_DEST" "$WEB_DEST" 2>/dev/null || true

echo "[5/6] Restaurando appsettings.Production.json do servidor..."
[ -f "$REPO_ROOT/appsettings.api.production.bak" ] && sudo cp "$REPO_ROOT/appsettings.api.production.bak" "$API_DEST/appsettings.Production.json" && rm -f "$REPO_ROOT/appsettings.api.production.bak"
[ -f "$REPO_ROOT/appsettings.web.production.bak" ] && sudo cp "$REPO_ROOT/appsettings.web.production.bak" "$WEB_DEST/appsettings.Production.json" && rm -f "$REPO_ROOT/appsettings.web.production.bak"

echo "[6/6] Reiniciando serviços..."
sudo systemctl restart nfse-api nfse-web

echo "Deploy concluído. API: $API_DEST | Web: $WEB_DEST"
sudo systemctl status nfse-api nfse-web --no-pager
