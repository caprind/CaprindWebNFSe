-- Script para adicionar colunas de validação de email na tabela Usuarios
-- Execute este script diretamente no banco de dados MySQL

USE nfs226;

-- Adiciona coluna CodigoValidacao se não existir
SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'nfs226' 
    AND TABLE_NAME = 'Usuarios' 
    AND COLUMN_NAME = 'CodigoValidacao');

SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE Usuarios ADD COLUMN CodigoValidacao VARCHAR(10) NULL', 
    'SELECT "Coluna CodigoValidacao já existe" AS Mensagem');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Adiciona coluna DataExpiracaoCodigo se não existir
SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'nfs226' 
    AND TABLE_NAME = 'Usuarios' 
    AND COLUMN_NAME = 'DataExpiracaoCodigo');

SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE Usuarios ADD COLUMN DataExpiracaoCodigo DATETIME(6) NULL', 
    'SELECT "Coluna DataExpiracaoCodigo já existe" AS Mensagem');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Adiciona coluna EmailValidado se não existir
SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'nfs226' 
    AND TABLE_NAME = 'Usuarios' 
    AND COLUMN_NAME = 'EmailValidado');

SET @sql = IF(@col_exists = 0, 
    'ALTER TABLE Usuarios ADD COLUMN EmailValidado TINYINT(1) NOT NULL DEFAULT 0', 
    'SELECT "Coluna EmailValidado já existe" AS Mensagem');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Atualiza registros existentes para ter EmailValidado = true (para não bloquear usuários já cadastrados)
UPDATE Usuarios SET EmailValidado = 1 WHERE EmailValidado = 0;

SELECT 'Colunas de validação de email adicionadas com sucesso!' AS Resultado;



