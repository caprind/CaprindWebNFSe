using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NFSe2026.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEmpresaIdEmTomador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            // Cria a tabela ConfiguracoesAPI apenas se não existir (SQL direto)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ConfiguracoesAPI` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `Ambiente` int NOT NULL,
                    `UrlBase` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
                    `ClientId` varchar(200) CHARACTER SET utf8mb4 NULL,
                    `ClientSecret` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `Scope` varchar(200) CHARACTER SET utf8mb4 NULL,
                    `Timeout` int NOT NULL,
                    `DataCriacao` datetime(6) NOT NULL,
                    `DataAtualizacao` datetime(6) NULL,
                    CONSTRAINT `PK_ConfiguracoesAPI` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Cria a tabela Empresas apenas se não existir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Empresas` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `CNPJ` varchar(14) CHARACTER SET utf8mb4 NOT NULL,
                    `RazaoSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                    `NomeFantasia` varchar(200) CHARACTER SET utf8mb4 NULL,
                    `InscricaoEstadual` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `InscricaoMunicipal` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `Endereco` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                    `Numero` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    `Complemento` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `Bairro` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `Cidade` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `UF` varchar(2) CHARACTER SET utf8mb4 NOT NULL,
                    `CodigoMunicipio` varchar(7) CHARACTER SET utf8mb4 NULL,
                    `CEP` varchar(8) CHARACTER SET utf8mb4 NOT NULL,
                    `Telefone` varchar(20) CHARACTER SET utf8mb4 NULL,
                    `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `CertificadoDigital` varchar(5000) CHARACTER SET utf8mb4 NULL,
                    `SenhaCertificado` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `Ambiente` int NOT NULL DEFAULT 0,
                    `SituacaoCadastral` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `Porte` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `NaturezaJuridica` varchar(200) CHARACTER SET utf8mb4 NULL,
                    `DataAbertura` datetime(6) NULL,
                    `Ativo` tinyint(1) NOT NULL DEFAULT 1,
                    `DataCriacao` datetime(6) NOT NULL,
                    `DataAtualizacao` datetime(6) NULL,
                    CONSTRAINT `PK_Empresas` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
            ");

            /* migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CNPJ = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RazaoSocial = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeFantasia = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InscricaoEstadual = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InscricaoMunicipal = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Complemento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bairro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UF = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CEP = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SituacaoCadastral = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Porte = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NaturezaJuridica = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAbertura = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            */

            // Criar tabela Prestadores apenas se não existir
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
                        `RazaoSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                        `NomeFantasia` varchar(200) CHARACTER SET utf8mb4 NULL,
                        `EmpresaId` int NOT NULL,
                        `CNPJ` varchar(14) CHARACTER SET utf8mb4 NOT NULL,
                        `InscricaoMunicipal` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                        `Endereco` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                        `Cidade` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                        `UF` varchar(2) CHARACTER SET utf8mb4 NOT NULL,
                        `CEP` varchar(8) CHARACTER SET utf8mb4 NOT NULL,
                        `Telefone` varchar(20) CHARACTER SET utf8mb4 NULL,
                        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
                        `CertificadoDigital` varchar(5000) CHARACTER SET utf8mb4 NULL,
                        `SenhaCertificado` varchar(500) CHARACTER SET utf8mb4 NULL,
                        `Ambiente` int NOT NULL,
                        `Ativo` tinyint(1) NOT NULL,
                        `DataCriacao` datetime(6) NOT NULL,
                        `DataAtualizacao` datetime(6) NULL,
                        CONSTRAINT `PK_Prestadores` PRIMARY KEY (`Id`),
                        CONSTRAINT `FK_Prestadores_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT
                    ) CHARACTER SET=utf8mb4;',
                    'SELECT ''Tabela Prestadores já existe'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_create;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
            
            /* COMENTADO - Agora usando SQL direto acima
            migrationBuilder.CreateTable(
                name: "Prestadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RazaoSocial = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeFantasia = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CNPJ = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InscricaoMunicipal = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UF = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CEP = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CertificadoDigital = table.Column<string>(type: "varchar(5000)", maxLength: 5000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenhaCertificado = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ambiente = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prestadores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            */

            // Criar tabela Tomadores apenas se não existir
            migrationBuilder.Sql(@"
                SET @table_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Tomadores'
                );
                
                SET @sql_create = IF(@table_exists = 0,
                    'CREATE TABLE `Tomadores` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `TipoPessoa` int NOT NULL,
                        `CPFCNPJ` varchar(14) CHARACTER SET utf8mb4 NOT NULL,
                        `RazaoSocialNome` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                        `InscricaoEstadual` varchar(50) CHARACTER SET utf8mb4 NULL,
                        `InscricaoMunicipal` varchar(50) CHARACTER SET utf8mb4 NULL,
                        `Endereco` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                        `Numero` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                        `Complemento` varchar(100) CHARACTER SET utf8mb4 NULL,
                        `Bairro` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                        `Cidade` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                        `UF` varchar(2) CHARACTER SET utf8mb4 NOT NULL,
                        `CEP` varchar(8) CHARACTER SET utf8mb4 NOT NULL,
                        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
                        `Telefone` varchar(20) CHARACTER SET utf8mb4 NULL,
                        `EmpresaId` int NOT NULL,
                        `DataCriacao` datetime(6) NOT NULL,
                        `DataAtualizacao` datetime(6) NULL,
                        CONSTRAINT `PK_Tomadores` PRIMARY KEY (`Id`),
                        CONSTRAINT `FK_Tomadores_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT
                    ) CHARACTER SET=utf8mb4;',
                    'SELECT ''Tabela Tomadores já existe'' AS Mensagem;'
                );
                
                PREPARE stmt FROM @sql_create;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
            
            /* COMENTADO - Agora usando SQL direto acima
            migrationBuilder.CreateTable(
                name: "Tomadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TipoPessoa = table.Column<int>(type: "int", nullable: false),
                    CPFCNPJ = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RazaoSocialNome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InscricaoEstadual = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InscricaoMunicipal = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Complemento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bairro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UF = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CEP = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tomadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tomadores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            */
            
            // Criar tabela Usuarios apenas se não existir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Usuarios` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `EmpresaId` int NOT NULL,
                    `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `Email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `SenhaHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `Telefone` varchar(20) CHARACTER SET utf8mb4 NULL,
                    `Ativo` tinyint(1) NOT NULL,
                    `DataCriacao` datetime(6) NOT NULL,
                    `DataAtualizacao` datetime(6) NULL,
                    `UltimoAcesso` datetime(6) NULL,
                    CONSTRAINT `PK_Usuarios` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_Usuarios_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT
                ) CHARACTER SET=utf8mb4;
            ");

            // Criar tabela NotasFiscais apenas se não existir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `NotasFiscais` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `PrestadorId` int NOT NULL,
                    `TomadorId` int NOT NULL,
                    `Numero` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `CodigoVerificacao` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `Serie` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
                    `Competencia` datetime(6) NOT NULL,
                    `DataEmissao` datetime(6) NOT NULL,
                    `DataVencimento` datetime(6) NULL,
                    `ValorServicos` decimal(18,2) NOT NULL,
                    `ValorDeducoes` decimal(18,2) NOT NULL,
                    `ValorPis` decimal(18,2) NOT NULL,
                    `ValorCofins` decimal(18,2) NOT NULL,
                    `ValorInss` decimal(18,2) NOT NULL,
                    `ValorIr` decimal(18,2) NOT NULL,
                    `ValorCsll` decimal(18,2) NOT NULL,
                    `ValorIss` decimal(18,2) NOT NULL,
                    `ValorIssRetido` decimal(18,2) NOT NULL,
                    `ValorLiquido` decimal(18,2) NOT NULL,
                    `Situacao` int NOT NULL,
                    `DiscriminacaoServicos` text CHARACTER SET utf8mb4 NOT NULL,
                    `CodigoMunicipio` varchar(7) CHARACTER SET utf8mb4 NOT NULL,
                    `Observacoes` text CHARACTER SET utf8mb4 NULL,
                    `XML` longtext CHARACTER SET utf8mb4 NULL,
                    `JSON` longtext CHARACTER SET utf8mb4 NULL,
                    `DataCriacao` datetime(6) NOT NULL,
                    `DataAtualizacao` datetime(6) NULL,
                    CONSTRAINT `PK_NotasFiscais` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_NotasFiscais_Prestadores_PrestadorId` FOREIGN KEY (`PrestadorId`) REFERENCES `Prestadores` (`Id`) ON DELETE RESTRICT,
                    CONSTRAINT `FK_NotasFiscais_Tomadores_TomadorId` FOREIGN KEY (`TomadorId`) REFERENCES `Tomadores` (`Id`) ON DELETE RESTRICT
                ) CHARACTER SET=utf8mb4;
            ");

            // Criar tabela ItensServico apenas se não existir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ItensServico` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `NotaFiscalId` int NOT NULL,
                    `CodigoServico` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    `Discriminacao` text CHARACTER SET utf8mb4 NOT NULL,
                    `Quantidade` decimal(10,2) NOT NULL,
                    `ValorUnitario` decimal(18,2) NOT NULL,
                    `ValorTotal` decimal(18,2) NOT NULL,
                    `AliquotaIss` decimal(5,2) NOT NULL,
                    `ItemListaServico` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
                    CONSTRAINT `PK_ItensServico` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_ItensServico_NotasFiscais_NotaFiscalId` FOREIGN KEY (`NotaFiscalId`) REFERENCES `NotasFiscais` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
            ");

            // Cria índice único apenas se não existir (SQL direto)
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.statistics 
                               WHERE table_schema = DATABASE() 
                               AND table_name = 'ConfiguracoesAPI' 
                               AND index_name = 'IX_ConfiguracoesAPI_Ambiente');
                SET @sqlstmt := IF(@exist = 0, 
                    'CREATE UNIQUE INDEX `IX_ConfiguracoesAPI_Ambiente` ON `ConfiguracoesAPI` (`Ambiente`)', 
                    'SELECT ''Index already exists'' AS result');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Criar índices apenas se não existirem
            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'Empresas' 
                             AND INDEX_NAME = 'IX_Empresas_CNPJ');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE UNIQUE INDEX `IX_Empresas_CNPJ` ON `Empresas` (`CNPJ`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'ItensServico' 
                             AND INDEX_NAME = 'IX_ItensServico_NotaFiscalId');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_ItensServico_NotaFiscalId` ON `ItensServico` (`NotaFiscalId`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'NotasFiscais' 
                             AND INDEX_NAME = 'IX_NotasFiscais_DataEmissao');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_NotasFiscais_DataEmissao` ON `NotasFiscais` (`DataEmissao`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'NotasFiscais' 
                             AND INDEX_NAME = 'IX_NotasFiscais_Numero');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_NotasFiscais_Numero` ON `NotasFiscais` (`Numero`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Índice IX_NotasFiscais_PrestadorId será criado apenas se a coluna PrestadorId existir
            // (essa coluna será removida na próxima migration)
            migrationBuilder.Sql(@"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'NotasFiscais' 
                                  AND COLUMN_NAME = 'PrestadorId');
                SET @idx_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                                  WHERE TABLE_SCHEMA = DATABASE() 
                                  AND TABLE_NAME = 'NotasFiscais' 
                                  AND INDEX_NAME = 'IX_NotasFiscais_PrestadorId');
                SET @sqlstmt = IF(@col_exists > 0 AND @idx_exists = 0, 
                    'CREATE INDEX `IX_NotasFiscais_PrestadorId` ON `NotasFiscais` (`PrestadorId`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'NotasFiscais' 
                             AND INDEX_NAME = 'IX_NotasFiscais_Situacao');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_NotasFiscais_Situacao` ON `NotasFiscais` (`Situacao`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'NotasFiscais' 
                             AND INDEX_NAME = 'IX_NotasFiscais_TomadorId');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_NotasFiscais_TomadorId` ON `NotasFiscais` (`TomadorId`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Criar índices de Tomadores apenas se não existirem
            migrationBuilder.Sql(@"
                SET @idx1 = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'Tomadores' 
                             AND INDEX_NAME = 'IX_Tomadores_CPFCNPJ');
                SET @sql1 = IF(@idx1 = 0, 'CREATE INDEX `IX_Tomadores_CPFCNPJ` ON `Tomadores` (`CPFCNPJ`);', 'SELECT 1;');
                SET @stmt1 = @sql1;
                PREPARE stmt1 FROM @stmt1;
                EXECUTE stmt1;
                DEALLOCATE PREPARE stmt1;
            ");

            migrationBuilder.Sql(@"
                SET @idx2 = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'Tomadores' 
                             AND INDEX_NAME = 'IX_Tomadores_EmpresaId');
                SET @sql2 = IF(@idx2 = 0, 'CREATE INDEX `IX_Tomadores_EmpresaId` ON `Tomadores` (`EmpresaId`);', 'SELECT 1;');
                SET @stmt2 = @sql2;
                PREPARE stmt2 FROM @stmt2;
                EXECUTE stmt2;
                DEALLOCATE PREPARE stmt2;
            ");

            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'Usuarios' 
                             AND INDEX_NAME = 'IX_Usuarios_Email');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_Usuarios_Email` ON `Usuarios` (`Email`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                             WHERE TABLE_SCHEMA = DATABASE() 
                             AND TABLE_NAME = 'Usuarios' 
                             AND INDEX_NAME = 'IX_Usuarios_EmpresaId');
                SET @sqlstmt = IF(@exist = 0, 
                    'CREATE INDEX `IX_Usuarios_EmpresaId` ON `Usuarios` (`EmpresaId`)', 
                    'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesAPI");

            migrationBuilder.DropTable(
                name: "ItensServico");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "NotasFiscais");

            migrationBuilder.DropTable(
                name: "Prestadores");

            migrationBuilder.DropTable(
                name: "Tomadores");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
