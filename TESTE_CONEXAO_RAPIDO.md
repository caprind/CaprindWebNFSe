# 🚀 Teste Rápido de Conexão - 3 Métodos

## ⚡ Método 1: Teste Rápido (Recomendado)

Execute a aplicação:

```bash
cd NFSe2026.API
dotnet run
```

A aplicação tentará conectar automaticamente. Procure no console:
- ✅ **"Database verified/created successfully"** = Conexão OK!
- ❌ **Erro de conexão** = Verifique as credenciais

## 🔧 Método 2: Via MySQL Command Line

```bash
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -pcap0902loc NFSe2026
```

Ou interativo (mais seguro):
```bash
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -p NFSe2026
```

Depois execute:
```sql
SHOW TABLES;
SELECT DATABASE();
SELECT NOW();
```

## 🎯 Método 3: Script Batch (Windows)

Execute:
```bash
database\TestarConexaoRapido.bat
```

## 📋 Sua Configuração Atual

Sua connection string está configurada:
- **Server:** `mysql02.caprind1.hospedagemdesites.ws`
- **Database:** `NFSe2026`
- **User:** `caprind11`
- **Port:** `3306`

## ✅ O que verificar se der erro:

1. **Servidor está acessível?**
   - Teste: `ping mysql02.caprind1.hospedagemdesites.ws`

2. **Credenciais estão corretas?**
   - Verifique usuário e senha

3. **Banco existe?**
   - O banco precisa ser criado no servidor primeiro

4. **Firewall/Rede?**
   - Verifique se a porta 3306 está acessível

## 🎯 Teste mais simples:

Abra um terminal e execute:
```bash
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -pcap0902loc -e "SELECT 1 as Teste"
```

Se retornar `1`, a conexão está funcionando! ✅

