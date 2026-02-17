-- Script para alterar o campo CertificadoDigital de varchar(5000) para LONGTEXT
-- Isso resolve o problema de certificados sendo truncados ao salvar

-- Verifica se a coluna existe e altera o tipo
SET @dbname = DATABASE();
SET @tablename = 'Empresas';
SET @columnname = 'CertificadoDigital';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE
      (TABLE_SCHEMA = @dbname)
      AND (TABLE_NAME = @tablename)
      AND (COLUMN_NAME = @columnname)
      AND (DATA_TYPE = 'varchar')
  ) > 0,
  CONCAT('ALTER TABLE `', @tablename, '` MODIFY COLUMN `', @columnname, '` LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL'),
  'SELECT "Coluna já está como LONGTEXT ou não existe" AS resultado'
));
PREPARE alterIfExists FROM @preparedStatement;
EXECUTE alterIfExists;
DEALLOCATE PREPARE alterIfExists;

SELECT 'Campo CertificadoDigital alterado para LONGTEXT com sucesso!' AS resultado;

