@echo off
echo ============================================
echo Teste Rapido de Conexao MySQL
echo ============================================
echo.

echo Testando conexao com o banco de dados...
echo Server: mysql02.caprind1.hospedagemdesites.ws
echo Database: NFSe2026
echo User: caprind11
echo.

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
    echo Verifique as credenciais e se o MySQL esta acessivel
    echo ============================================
)

pause

