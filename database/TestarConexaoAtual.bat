@echo off
echo ============================================
echo Teste de Conexao MySQL - Servidor Atual
echo ============================================
echo.

echo Servidor: nfs226.mysql.dbaas.com.br
echo Database: nfs226
echo Usuario: nfs226
echo.

echo 1. Testando ping...
ping -n 2 nfs226.mysql.dbaas.com.br
echo.

echo 2. Testando conexao MySQL...
mysql -h nfs226.mysql.dbaas.com.br -u nfs226 -pC@p0902loc nfs226 -e "SELECT 'Conexao OK!' as Status, DATABASE() as Banco, NOW() as DataHora; SHOW TABLES;"

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
    echo.
    echo Verifique o arquivo TROUBLESHOOTING_CONEXAO.md
    echo ============================================
)

pause

