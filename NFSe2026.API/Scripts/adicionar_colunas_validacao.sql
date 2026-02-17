-- Script SQL para adicionar colunas de validação de email
-- Execute este script diretamente no MySQL

-- Adiciona coluna CodigoValidacao
ALTER TABLE Usuarios 
ADD COLUMN CodigoValidacao VARCHAR(10) NULL;

-- Adiciona coluna DataExpiracaoCodigo
ALTER TABLE Usuarios 
ADD COLUMN DataExpiracaoCodigo DATETIME(6) NULL;

-- Adiciona coluna EmailValidado com valor padrão false
ALTER TABLE Usuarios 
ADD COLUMN EmailValidado TINYINT(1) NOT NULL DEFAULT 0;

-- Atualiza usuários existentes para ter EmailValidado = true (para não bloquear usuários já cadastrados)
UPDATE Usuarios SET EmailValidado = 1 WHERE EmailValidado = 0;



