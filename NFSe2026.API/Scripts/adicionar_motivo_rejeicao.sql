-- Script para adicionar campo MotivoRejeicao na tabela NotasFiscais
-- Execute este script se as migrations não funcionarem devido a tabelas já existentes

USE nfs226;

-- Adiciona a coluna MotivoRejeicao se não existir
ALTER TABLE `NotasFiscais` 
ADD COLUMN IF NOT EXISTS `MotivoRejeicao` text CHARACTER SET utf8mb4 NULL;

-- Se o comando acima não funcionar (MySQL versão antiga), use:
-- ALTER TABLE `NotasFiscais` ADD COLUMN `MotivoRejeicao` text CHARACTER SET utf8mb4 NULL;

SELECT 'Campo MotivoRejeicao adicionado com sucesso!' AS Resultado;



