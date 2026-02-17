-- =============================================
-- Script para adicionar coluna NsNRec na tabela NotasFiscais
-- =============================================

USE nfs226;

-- Verifica se a coluna já existe antes de adicionar
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'nfs226' 
    AND TABLE_NAME = 'NotasFiscais' 
    AND COLUMN_NAME = 'NsNRec'
);

SET @sql_add_column = IF(@column_exists = 0,
    'ALTER TABLE NotasFiscais ADD COLUMN NsNRec VARCHAR(100) NULL CHARACTER SET utf8mb4;',
    'SELECT ''Coluna NsNRec já existe na tabela NotasFiscais'' AS Mensagem;'
);

PREPARE stmt FROM @sql_add_column;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

