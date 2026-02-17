-- =============================================
-- Script de Teste de Conexão ao Banco de Dados
-- =============================================

-- 1. Verificar se consegue conectar ao MySQL
SELECT VERSION() as 'Versão MySQL';
SELECT DATABASE() as 'Banco Atual';
SELECT USER() as 'Usuário Atual';
SELECT NOW() as 'Data/Hora Atual';

-- 2. Verificar se o banco NFSe2026 existe
SHOW DATABASES LIKE 'NFSe2026';

-- 3. Selecionar o banco
USE NFSe2026;

-- 4. Verificar se todas as tabelas foram criadas
SHOW TABLES;

-- 5. Verificar estrutura de uma tabela (exemplo: Empresas)
DESCRIBE Empresas;

-- 6. Verificar charset do banco
SHOW CREATE DATABASE NFSe2026;

-- 7. Contar registros nas tabelas
SELECT 
    'Empresas' as Tabela, COUNT(*) as Registros FROM Empresas
UNION ALL
SELECT 'Usuarios', COUNT(*) FROM Usuarios
UNION ALL
SELECT 'Prestadores', COUNT(*) FROM Prestadores
UNION ALL
SELECT 'Tomadores', COUNT(*) FROM Tomadores
UNION ALL
SELECT 'NotasFiscais', COUNT(*) FROM NotasFiscais
UNION ALL
SELECT 'ItensServico', COUNT(*) FROM ItensServico
UNION ALL
SELECT 'ConfiguracoesAPI', COUNT(*) FROM ConfiguracoesAPI;

-- 8. Verificar dados iniciais (ConfiguracoesAPI)
SELECT * FROM ConfiguracoesAPI;

