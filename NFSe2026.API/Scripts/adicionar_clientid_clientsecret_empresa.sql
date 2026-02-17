-- Script SQL para adicionar campos ClientId e ClientSecret na tabela Empresas
-- Execute este script manualmente se a migration não for aplicada automaticamente

-- Verifica se as colunas já existem antes de adicionar
SET @col_exists_clientid = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'Empresas' 
    AND COLUMN_NAME = 'ClientId'
);

SET @col_exists_clientsecret = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'Empresas' 
    AND COLUMN_NAME = 'ClientSecret'
);

-- Adiciona ClientId se não existir
SET @sql_clientid = IF(@col_exists_clientid = 0,
    'ALTER TABLE `Empresas` ADD COLUMN `ClientId` VARCHAR(500) NULL COMMENT ''ClientId da API Nacional NFSe (criptografado)'';',
    'SELECT ''Coluna ClientId já existe'' AS Mensagem;'
);

PREPARE stmt FROM @sql_clientid;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Adiciona ClientSecret se não existir
SET @sql_clientsecret = IF(@col_exists_clientsecret = 0,
    'ALTER TABLE `Empresas` ADD COLUMN `ClientSecret` VARCHAR(500) NULL COMMENT ''ClientSecret da API Nacional NFSe (criptografado)'';',
    'SELECT ''Coluna ClientSecret já existe'' AS Mensagem;'
);

PREPARE stmt FROM @sql_clientsecret;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verifica se as colunas foram criadas
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'Empresas'
AND COLUMN_NAME IN ('ClientId', 'ClientSecret');

