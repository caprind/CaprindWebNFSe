-- Script SQL para adicionar campo PDFUrl na tabela NotasFiscais
-- Execute este script se a migration não for aplicada automaticamente

-- Verifica se a coluna já existe antes de adicionar
SET @col_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'NotasFiscais'
    AND COLUMN_NAME = 'PDFUrl'
);

-- Adiciona PDFUrl se não existir
SET @sql_add_col = IF(@col_exists = 0,
    'ALTER TABLE `NotasFiscais` ADD COLUMN `PDFUrl` TEXT NULL COMMENT ''URL do PDF da nota fiscal (quando disponível)'';',
    'SELECT ''Coluna PDFUrl já existe'' AS Mensagem;'
);

PREPARE stmt FROM @sql_add_col;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verifica se a coluna foi criada
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    COLUMN_TYPE,
    IS_NULLABLE,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'NotasFiscais'
AND COLUMN_NAME = 'PDFUrl';

