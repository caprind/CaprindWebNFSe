USE nfs226;

-- Verifica e adiciona DataVencimentoCertificado
SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
               WHERE TABLE_SCHEMA = 'nfs226' 
               AND TABLE_NAME = 'Empresas' 
               AND COLUMN_NAME = 'DataVencimentoCertificado');
SET @sqlstmt := IF(@exist = 0, 
    'ALTER TABLE `Empresas` ADD COLUMN `DataVencimentoCertificado` datetime(6) NULL', 
    'SELECT ''Coluna já existe'' AS result');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT 'Coluna DataVencimentoCertificado adicionada com sucesso (se não existia)' AS resultado;



