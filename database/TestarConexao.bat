@echo off
echo ============================================
echo Teste de Conexao ao Banco de Dados MySQL
echo ============================================
echo.

echo Testando conexao MySQL...
echo Digite a senha do MySQL quando solicitado
echo.

mysql -u root -p -e "SELECT VERSION() as 'Versao MySQL'; SELECT DATABASE() as 'Banco Atual'; SELECT USER() as 'Usuario'; SELECT NOW() as 'Data_Hora';"

echo.
echo Testando acesso ao banco NFSe2026...
mysql -u root -p NFSe2026 -e "SHOW TABLES;"

echo.
echo ============================================
echo Teste concluido!
echo ============================================
pause

