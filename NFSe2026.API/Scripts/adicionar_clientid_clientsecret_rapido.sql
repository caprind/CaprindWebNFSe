-- Script SQL RÁPIDO para adicionar campos ClientId e ClientSecret na tabela Empresas
-- Execute este script diretamente no banco de dados MySQL para resolver o erro de login imediatamente

-- Adiciona ClientId se não existir (MySQL não suporta IF NOT EXISTS diretamente)
SET @col_exists_clientid = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'Empresas' 
    AND COLUMN_NAME = 'ClientId'
);

SET @sql_clientid = IF(@col_exists_clientid = 0,
    'ALTER TABLE `Empresas` ADD COLUMN `ClientId` VARCHAR(500) NULL COMMENT ''ClientId da API Nacional NFSe (criptografado)'';',
    'SELECT ''Coluna ClientId já existe'' AS Mensagem;'
);

PREPARE stmt FROM @sql_clientid;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Adiciona ClientSecret se não existir
SET @col_exists_clientsecret = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_NAME = 'Empresas' 
    AND COLUMN_NAME = 'ClientSecret'
);

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
