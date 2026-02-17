using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFSe2026.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMotivoRejeicaoEmNotaFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "EmailValidado",
                table: "Usuarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'NotasFiscais' 
                                  AND COLUMN_NAME = 'MotivoRejeicao');
                SET @sqlstmt = IF(@col_exists = 0, 
                    'ALTER TABLE `NotasFiscais` ADD `MotivoRejeicao` text CHARACTER SET utf8mb4 NULL', 
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
                name: "MotivoRejeicao",
                table: "NotasFiscais");

            migrationBuilder.AlterColumn<bool>(
                name: "EmailValidado",
                table: "Usuarios",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);
        }
    }
}
