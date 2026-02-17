-- Script simples para adicionar campos de tributação na tabela Empresas
-- Execute este script diretamente no MySQL

USE nfs226;

-- Adiciona as colunas (execute um por vez se houver erro de coluna já existir)
ALTER TABLE `Empresas` ADD COLUMN `RegimeEspecialTributacao` varchar(50) CHARACTER SET utf8mb4 NULL DEFAULT 'Nenhum';
ALTER TABLE `Empresas` ADD COLUMN `OptanteSimplesNacional` tinyint(1) NOT NULL DEFAULT 1;
ALTER TABLE `Empresas` ADD COLUMN `IncentivoFiscal` tinyint(1) NOT NULL DEFAULT 0;

-- Atualiza valores padrão para empresas existentes
UPDATE `Empresas` SET `RegimeEspecialTributacao` = 'Nenhum' WHERE `RegimeEspecialTributacao` IS NULL;
UPDATE `Empresas` SET `OptanteSimplesNacional` = 1 WHERE `OptanteSimplesNacional` IS NULL;
UPDATE `Empresas` SET `IncentivoFiscal` = 0 WHERE `IncentivoFiscal` IS NULL;

SELECT 'Campos de tributação adicionados com sucesso!' AS Resultado;



