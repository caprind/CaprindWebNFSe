CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE TABLE `ConfiguracoesAPI` (
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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE TABLE `Empresas` (
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
        `CEP` varchar(8) CHARACTER SET utf8mb4 NOT NULL,
        `Telefone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SituacaoCadastral` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Porte` varchar(50) CHARACTER SET utf8mb4 NULL,
        `NaturezaJuridica` varchar(200) CHARACTER SET utf8mb4 NULL,
        `DataAbertura` datetime(6) NULL,
        `Ativo` tinyint(1) NOT NULL,
        `DataCriacao` datetime(6) NOT NULL,
        `DataAtualizacao` datetime(6) NULL,
        CONSTRAINT `PK_Empresas` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE TABLE `Prestadores` (
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
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE TABLE `Tomadores` (
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
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE TABLE `Usuarios` (
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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE TABLE `NotasFiscais` (
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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE TABLE `ItensServico` (
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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE UNIQUE INDEX `IX_ConfiguracoesAPI_Ambiente` ON `ConfiguracoesAPI` (`Ambiente`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE UNIQUE INDEX `IX_Empresas_CNPJ` ON `Empresas` (`CNPJ`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_ItensServico_NotaFiscalId` ON `ItensServico` (`NotaFiscalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_NotasFiscais_DataEmissao` ON `NotasFiscais` (`DataEmissao`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_NotasFiscais_Numero` ON `NotasFiscais` (`Numero`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_NotasFiscais_PrestadorId` ON `NotasFiscais` (`PrestadorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_NotasFiscais_Situacao` ON `NotasFiscais` (`Situacao`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_NotasFiscais_TomadorId` ON `NotasFiscais` (`TomadorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_Prestadores_CNPJ` ON `Prestadores` (`CNPJ`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_Prestadores_EmpresaId` ON `Prestadores` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_Prestadores_InscricaoMunicipal` ON `Prestadores` (`InscricaoMunicipal`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_Tomadores_CPFCNPJ` ON `Tomadores` (`CPFCNPJ`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_Tomadores_EmpresaId` ON `Tomadores` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_Usuarios_Email` ON `Usuarios` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    CREATE INDEX `IX_Usuarios_EmpresaId` ON `Usuarios` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228005835_AdicionarEmpresaIdEmTomador') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251228005835_AdicionarEmpresaIdEmTomador', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `Empresas` ADD `Ambiente` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `Empresas` ADD `CertificadoDigital` varchar(5000) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `Empresas` ADD `SenhaCertificado` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN


                    UPDATE Empresas e
                    INNER JOIN Prestadores p ON e.Id = p.EmpresaId
                    SET 
                        e.CertificadoDigital = COALESCE(p.CertificadoDigital, e.CertificadoDigital),
                        e.SenhaCertificado = COALESCE(p.SenhaCertificado, e.SenhaCertificado),
                        e.Ambiente = p.Ambiente
                    WHERE p.Ativo = 1;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `NotasFiscais` ADD `EmpresaId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN


                    UPDATE NotasFiscais nf
                    INNER JOIN Prestadores p ON nf.PrestadorId = p.Id
                    SET nf.EmpresaId = p.EmpresaId;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `NotasFiscais` DROP FOREIGN KEY `FK_NotasFiscais_Prestadores_PrestadorId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `NotasFiscais` DROP INDEX `IX_NotasFiscais_PrestadorId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `NotasFiscais` DROP COLUMN `PrestadorId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `NotasFiscais` MODIFY COLUMN `EmpresaId` int NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    CREATE INDEX `IX_NotasFiscais_EmpresaId` ON `NotasFiscais` (`EmpresaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    ALTER TABLE `NotasFiscais` ADD CONSTRAINT `FK_NotasFiscais_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    DROP TABLE `Prestadores`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228011727_RemoverPrestadorEAtualizarNotaFiscal') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251228011727_RemoverPrestadorEAtualizarNotaFiscal', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228171920_AdicionarCodigoMunicipioEmEmpresa') THEN

    ALTER TABLE `Empresas` ADD `CodigoMunicipio` varchar(7) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228171920_AdicionarCodigoMunicipioEmEmpresa') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251228171920_AdicionarCodigoMunicipioEmEmpresa', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228175933_AdicionarLogotipoEmEmpresa') THEN

    ALTER TABLE `Empresas` MODIFY COLUMN `CodigoMunicipio` varchar(7) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228175933_AdicionarLogotipoEmEmpresa') THEN

    ALTER TABLE `Empresas` ADD `Logotipo` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228175933_AdicionarLogotipoEmEmpresa') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251228175933_AdicionarLogotipoEmEmpresa', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228184549_AdicionarValidacaoEmailEmUsuario') THEN

    ALTER TABLE `Usuarios` ADD `CodigoValidacao` varchar(10) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228184549_AdicionarValidacaoEmailEmUsuario') THEN

    ALTER TABLE `Usuarios` ADD `DataExpiracaoCodigo` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228184549_AdicionarValidacaoEmailEmUsuario') THEN

    ALTER TABLE `Usuarios` ADD `EmailValidado` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251228184549_AdicionarValidacaoEmailEmUsuario') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251228184549_AdicionarValidacaoEmailEmUsuario', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

