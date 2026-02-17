using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFSe2026.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarValidacaoEmailEmUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Usuarios' 
                                  AND COLUMN_NAME = 'CodigoValidacao');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Usuarios` ADD `CodigoValidacao` varchar(10) CHARACTER SET utf8mb4 NULL', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Usuarios' 
                                  AND COLUMN_NAME = 'DataExpiracaoCodigo');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Usuarios` ADD `DataExpiracaoCodigo` datetime(6) NULL', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Usuarios' 
                                  AND COLUMN_NAME = 'EmailValidado');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Usuarios` ADD `EmailValidado` tinyint(1) NOT NULL DEFAULT 0', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoValidacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataExpiracaoCodigo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailValidado",
                table: "Usuarios");
        }
    }
}
