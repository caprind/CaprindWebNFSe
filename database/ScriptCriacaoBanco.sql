-- =============================================
-- Script Simples: Criar apenas o Banco de Dados
-- Execute este script primeiro, depois use o ScriptCompleto.sql ou Migrations
-- =============================================

CREATE DATABASE IF NOT EXISTS NFSe2026
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

-- Verificar criação
SHOW DATABASES LIKE 'NFSe2026';

-- Informações do banco
SELECT 
    SCHEMA_NAME as 'Database',
    DEFAULT_CHARACTER_SET_NAME as 'Charset',
    DEFAULT_COLLATION_NAME as 'Collation'
FROM information_schema.SCHEMATA 
WHERE SCHEMA_NAME = 'NFSe2026';

