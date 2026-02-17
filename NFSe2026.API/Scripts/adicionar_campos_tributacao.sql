-- Script para adicionar campos de tributação na tabela Empresas
-- Execute este script se as migrations não funcionarem devido a tabelas já existentes

USE nfs226;

-- Adiciona a coluna RegimeEspecialTributacao
-- Verifica se a coluna já existe antes de adicionar
SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
               WHERE TABLE_SCHEMA = DATABASE() 
               AND TABLE_NAME = 'Empresas' 
               AND COLUMN_NAME = 'RegimeEspecialTributacao');
SET @sqlstmt := IF(@exist = 0, 
    'ALTER TABLE `Empresas` ADD COLUMN `RegimeEspecialTributacao` varchar(50) CHARACTER SET utf8mb4 NULL DEFAULT ''Nenhum''', 
    'SELECT ''Coluna RegimeEspecialTributacao já existe'' AS result');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Adiciona a coluna OptanteSimplesNacional
SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
               WHERE TABLE_SCHEMA = DATABASE() 
               AND TABLE_NAME = 'Empresas' 
               AND COLUMN_NAME = 'OptanteSimplesNacional');
SET @sqlstmt := IF(@exist = 0, 
    'ALTER TABLE `Empresas` ADD COLUMN `OptanteSimplesNacional` tinyint(1) NOT NULL DEFAULT 1', 
    'SELECT ''Coluna OptanteSimplesNacional já existe'' AS result');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Adiciona a coluna IncentivoFiscal
SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
               WHERE TABLE_SCHEMA = DATABASE() 
               AND TABLE_NAME = 'Empresas' 
               AND COLUMN_NAME = 'IncentivoFiscal');
SET @sqlstmt := IF(@exist = 0, 
    'ALTER TABLE `Empresas` ADD COLUMN `IncentivoFiscal` tinyint(1) NOT NULL DEFAULT 0', 
    'SELECT ''Coluna IncentivoFiscal já existe'' AS result');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Atualiza valores padrão para empresas existentes (se necessário)
UPDATE `Empresas` 
SET `RegimeEspecialTributacao` = 'Nenhum' 
WHERE `RegimeEspecialTributacao` IS NULL;

UPDATE `Empresas` 
SET `OptanteSimplesNacional` = 1 
WHERE `OptanteSimplesNacional` IS NULL;

UPDATE `Empresas` 
SET `IncentivoFiscal` = 0 
WHERE `IncentivoFiscal` IS NULL;

SELECT 'Campos de tributação adicionados com sucesso!' AS Resultado;

