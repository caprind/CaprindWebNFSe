using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFSe2026.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarNsNRecEmNotaFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'NotasFiscais' 
                                  AND COLUMN_NAME = 'NsNRec');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `NotasFiscais` ADD `NsNRec` varchar(100) CHARACTER SET utf8mb4 NULL', 
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
                name: "NsNRec",
                table: "NotasFiscais");
        }
    }
}
