@echo off
echo ============================================
echo Teste de Conexao MySQL - Servidor Remoto
echo ============================================
echo.

echo Testando servidor: mysql02.caprind1.hospedagemdesites.ws
echo Usuario: caprind11
echo Banco: NFSe2026
echo.

echo 1. Testando ping...
ping -n 2 mysql02.caprind1.hospedagemdesites.ws
echo.

echo 2. Testando conexao MySQL...
mysql -h mysql02.caprind1.hospedagemdesites.ws -u caprind11 -pcap0902loc NFSe2026 -e "SELECT 'Conexao OK!' as Status, DATABASE() as Banco, NOW() as DataHora; SHOW TABLES;"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo Conexao SUCESSO!
    echo ============================================
) else (
    echo.
    echo ============================================
    echo Erro na conexao!
    echo.
    echo Possiveis causas:
    echo - Servidor MySQL nao esta acessivel
    echo - Firewall bloqueando conexao
    echo - Credenciais incorretas
    echo - Banco de dados nao existe
    echo - IP nao esta na whitelist do servidor
    echo ============================================
)

pause

