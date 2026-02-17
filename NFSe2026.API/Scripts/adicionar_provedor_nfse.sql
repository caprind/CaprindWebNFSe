-- Script SQL para adicionar campo ProvedorNFSe na tabela Empresas
-- Execute este script se a migration não for aplicada automaticamente

-- Verifica se a coluna já existe antes de adicionar
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'Empresas' 
    AND COLUMN_NAME = 'ProvedorNFSe'
);

-- Adiciona ProvedorNFSe se não existir
SET @sql_add_col = IF(@col_exists = 0,
    'ALTER TABLE `Empresas` ADD COLUMN `ProvedorNFSe` int NOT NULL DEFAULT 1 COMMENT ''Provedor de NFSe (1=Nacional, 2=NS Tecnologia)'';',
    'SELECT ''Coluna ProvedorNFSe já existe'' AS Mensagem;'
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
    COLUMN_DEFAULT,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'Empresas'
AND COLUMN_NAME = 'ProvedorNFSe';

