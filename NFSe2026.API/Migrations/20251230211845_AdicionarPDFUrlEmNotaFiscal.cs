using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFSe2026.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPDFUrlEmNotaFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'NotasFiscais' 
                                  AND COLUMN_NAME = 'PDFUrl');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `NotasFiscais` ADD `PDFUrl` text CHARACTER SET utf8mb4 NULL', 
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
                name: "PDFUrl",
                table: "NotasFiscais");
        }
    }
}
