using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFSe2026.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarClientIdClientSecretEmEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Empresas' 
                                  AND COLUMN_NAME = 'ClientId');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Empresas` ADD `ClientId` varchar(500) CHARACTER SET utf8mb4 NULL', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'Empresas' 
                                  AND COLUMN_NAME = 'ClientSecret');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `Empresas` ADD `ClientSecret` varchar(500) CHARACTER SET utf8mb4 NULL', 
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
                name: "ClientId",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "Empresas");
        }
    }
}
