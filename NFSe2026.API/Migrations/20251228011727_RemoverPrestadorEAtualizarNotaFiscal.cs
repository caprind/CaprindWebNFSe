using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFSe2026.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoverPrestadorEAtualizarNotaFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adicionar campos em Empresas primeiro (apenas se não existirem)
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Empresas' 
                                  AND COLUMN_NAME = 'Ambiente');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Empresas` ADD `Ambiente` int NOT NULL DEFAULT 0', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Empresas' 
                                  AND COLUMN_NAME = 'CertificadoDigital');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Empresas` ADD `CertificadoDigital` varchar(5000) CHARACTER SET utf8mb4 NULL', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Empresas' 
                                  AND COLUMN_NAME = 'SenhaCertificado');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Empresas` ADD `SenhaCertificado` varchar(500) CHARACTER SET utf8mb4 NULL', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 2. Migrar dados de Prestadores para Empresas (se houver dados e a tabela existir)
            migrationBuilder.Sql(@"
                SET @table_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Prestadores'
                );
                
                SET @sql_migrate = IF(@table_exists > 0,
                    'UPDATE Empresas AS e
                    INNER JOIN Prestadores p ON e.Id = p.EmpresaId
                    SET 
                        e.CertificadoDigital = COALESCE(p.CertificadoDigital, e.CertificadoDigital),
                        e.SenhaCertificado = COALESCE(p.SenhaCertificado, e.SenhaCertificado),
                        e.Ambiente = p.Ambiente
                    WHERE p.Ativo = 1;',
                    'SELECT ''Tabela Prestadores não existe, pulando migração de dados'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_migrate;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 3. Adicionar coluna temporária EmpresaId em NotasFiscais (apenas se não existir)
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'NotasFiscais' 
                                  AND COLUMN_NAME = 'EmpresaId');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `NotasFiscais` ADD `EmpresaId` int NULL', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 4. Migrar dados: PrestadorId -> EmpresaId (através do Prestador.EmpresaId) - apenas se a tabela Prestadores existir e a coluna PrestadorId existir
            migrationBuilder.Sql(@"
                SET @table_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Prestadores'
                );
                
                SET @col_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'NotasFiscais' 
                    AND COLUMN_NAME = 'PrestadorId'
                );
                
                SET @sql_migrate_notas = IF(@table_exists > 0 AND @col_exists > 0,
                    'UPDATE NotasFiscais AS nf
                    INNER JOIN Prestadores p ON nf.PrestadorId = p.Id
                    SET nf.EmpresaId = p.EmpresaId;',
                    'SELECT ''Tabela Prestadores ou coluna PrestadorId não existe, pulando migração de NotasFiscais'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_migrate_notas;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 5. Remover foreign key antiga (se existir)
            migrationBuilder.Sql(@"
                SET @fk_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'NotasFiscais' 
                    AND CONSTRAINT_NAME = 'FK_NotasFiscais_Prestadores_PrestadorId'
                );
                
                SET @sql_drop_fk = IF(@fk_exists > 0,
                    'ALTER TABLE `NotasFiscais` DROP FOREIGN KEY `FK_NotasFiscais_Prestadores_PrestadorId`;',
                    'SELECT ''Foreign key FK_NotasFiscais_Prestadores_PrestadorId não existe'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_drop_fk;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 6. Remover índice antigo (se existir)
            migrationBuilder.Sql(@"
                SET @index_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.STATISTICS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'NotasFiscais' 
                    AND INDEX_NAME = 'IX_NotasFiscais_PrestadorId'
                );
                
                SET @sql_drop_idx = IF(@index_exists > 0,
                    'ALTER TABLE `NotasFiscais` DROP INDEX `IX_NotasFiscais_PrestadorId`;',
                    'SELECT ''Índice IX_NotasFiscais_PrestadorId não existe'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_drop_idx;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 7. Remover coluna PrestadorId (se existir)
            migrationBuilder.Sql(@"
                SET @col_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'NotasFiscais' 
                    AND COLUMN_NAME = 'PrestadorId'
                );
                
                SET @sql_drop_col = IF(@col_exists > 0,
                    'ALTER TABLE `NotasFiscais` DROP COLUMN `PrestadorId`;',
                    'SELECT ''Coluna PrestadorId não existe em NotasFiscais'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_drop_col;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 8. Tornar EmpresaId NOT NULL (apenas se ainda for nullable)
            migrationBuilder.Sql(@"
                SET @col_nullable = (SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS 
                                   WHERE TABLE_SCHEMA = DATABASE() 
                                   AND TABLE_NAME = 'NotasFiscais' 
                                   AND COLUMN_NAME = 'EmpresaId');
                SET @sqlstmt = IF(@col_nullable = 'YES', 
                    'ALTER TABLE `NotasFiscais` MODIFY COLUMN `EmpresaId` int NOT NULL', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 9. Criar novo índice (apenas se não existir)
            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'NotasFiscais' 
                             AND INDEX_NAME = 'IX_NotasFiscais_EmpresaId');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_NotasFiscais_EmpresaId` ON `NotasFiscais` (`EmpresaId`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 10. Criar nova foreign key (apenas se não existir)
            migrationBuilder.Sql(@"
                SET @fk_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                                 WHERE TABLE_SCHEMA = DATABASE() 
                                 AND TABLE_NAME = 'NotasFiscais' 
                                 AND CONSTRAINT_NAME = 'FK_NotasFiscais_Empresas_EmpresaId');
                SET @sqlstmt = IF(@fk_exists = 0, 
                    'ALTER TABLE `NotasFiscais` ADD CONSTRAINT `FK_NotasFiscais_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // 11. Remover tabela Prestadores (se existir)
            migrationBuilder.Sql(@"
                SET @table_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Prestadores'
                );
                
                SET @sql_drop_table = IF(@table_exists > 0,
                    'DROP TABLE IF EXISTS `Prestadores`;',
                    'SELECT ''Tabela Prestadores não existe, nada a fazer'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_drop_table;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotasFiscais_Empresas_EmpresaId",
                table: "NotasFiscais");

            migrationBuilder.DropColumn(
                name: "Ambiente",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "CertificadoDigital",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "SenhaCertificado",
                table: "Empresas");

            migrationBuilder.RenameColumn(
                name: "EmpresaId",
                table: "NotasFiscais",
                newName: "PrestadorId");

            migrationBuilder.RenameIndex(
                name: "IX_NotasFiscais_EmpresaId",
                table: "NotasFiscais",
                newName: "IX_NotasFiscais_PrestadorId");

            // Verificar se a tabela Prestadores já existe antes de criar
            migrationBuilder.Sql(@"
                SET @table_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Prestadores'
                );
                
                SET @sql_create = IF(@table_exists = 0,
                    'CREATE TABLE `Prestadores` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `EmpresaId` int NOT NULL,
                        `Ambiente` int NOT NULL,
                        `Ativo` tinyint(1) NOT NULL,
                        `CEP` varchar(8) CHARACTER SET utf8mb4 NOT NULL,
                        `CNPJ` varchar(14) CHARACTER SET utf8mb4 NOT NULL,
                        `CertificadoDigital` varchar(5000) CHARACTER SET utf8mb4 NULL,
                        `Cidade` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                        `DataAtualizacao` datetime(6) NULL,
                        `DataCriacao` datetime(6) NOT NULL,
                        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
                        `Endereco` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                        `InscricaoMunicipal` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                        `NomeFantasia` varchar(200) CHARACTER SET utf8mb4 NULL,
                        `RazaoSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                        `SenhaCertificado` varchar(500) CHARACTER SET utf8mb4 NULL,
                        `Telefone` varchar(20) CHARACTER SET utf8mb4 NULL,
                        `UF` varchar(2) CHARACTER SET utf8mb4 NOT NULL,
                        CONSTRAINT `PK_Prestadores` PRIMARY KEY (`Id`),
                        CONSTRAINT `FK_Prestadores_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT
                    ) CHARACTER SET=utf8mb4;',
                    'SELECT ''Tabela Prestadores já existe'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_create;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
            
            // Criar índices apenas se a tabela foi criada
            migrationBuilder.Sql(@"
                SET @table_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Prestadores'
                );
                
                SET @sql_create_idx = IF(@table_exists > 0,
                    'CREATE INDEX IF NOT EXISTS `IX_Prestadores_CNPJ` ON `Prestadores` (`CNPJ`);
                     CREATE INDEX IF NOT EXISTS `IX_Prestadores_EmpresaId` ON `Prestadores` (`EmpresaId`);
                     CREATE INDEX IF NOT EXISTS `IX_Prestadores_InscricaoMunicipal` ON `Prestadores` (`InscricaoMunicipal`);',
                    'SELECT ''Tabela Prestadores não existe, não é possível criar índices'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_create_idx;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
            
            // Adicionar foreign key apenas se a tabela Prestadores existir
            migrationBuilder.Sql(@"
                SET @table_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Prestadores'
                );
                
                SET @fk_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'NotasFiscais' 
                    AND CONSTRAINT_NAME = 'FK_NotasFiscais_Prestadores_PrestadorId'
                );
                
                SET @sql_add_fk = IF(@table_exists > 0 AND @fk_exists = 0,
                    'ALTER TABLE `NotasFiscais` ADD CONSTRAINT `FK_NotasFiscais_Prestadores_PrestadorId` FOREIGN KEY (`PrestadorId`) REFERENCES `Prestadores` (`Id`) ON DELETE RESTRICT;',
                    'SELECT ''Foreign key FK_NotasFiscais_Prestadores_PrestadorId já existe ou tabela Prestadores não existe'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_add_fk;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}
