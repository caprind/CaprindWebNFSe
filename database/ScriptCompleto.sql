-- =============================================
-- Script de Criação Completo do Banco de Dados NFSe2026
-- MySQL 8.0+
-- =============================================

-- Criar banco de dados
CREATE DATABASE IF NOT EXISTS NFSe2026
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE NFSe2026;

-- =============================================
-- Tabela: Empresas
-- =============================================
CREATE TABLE IF NOT EXISTS Empresas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CNPJ VARCHAR(14) NOT NULL UNIQUE,
    RazaoSocial VARCHAR(200) NOT NULL,
    NomeFantasia VARCHAR(200),
    InscricaoEstadual VARCHAR(50),
    InscricaoMunicipal VARCHAR(50),
    Endereco VARCHAR(200) NOT NULL,
    Numero VARCHAR(20) NOT NULL,
    Complemento VARCHAR(100),
    Bairro VARCHAR(100) NOT NULL,
    Cidade VARCHAR(100) NOT NULL,
    UF VARCHAR(2) NOT NULL,
    CEP VARCHAR(8) NOT NULL,
    Telefone VARCHAR(20),
    Email VARCHAR(100),
    SituacaoCadastral VARCHAR(50),
    Porte VARCHAR(50),
    NaturezaJuridica VARCHAR(200),
    DataAbertura DATE,
    Ativo TINYINT(1) NOT NULL DEFAULT 1,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME,
    INDEX idx_cnpj (CNPJ)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Tabela: Usuarios
-- =============================================
CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EmpresaId INT NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    SenhaHash VARCHAR(255) NOT NULL,
    Telefone VARCHAR(20),
    Ativo TINYINT(1) NOT NULL DEFAULT 1,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME,
    UltimoAcesso DATETIME,
    INDEX idx_email (Email),
    INDEX idx_empresa (EmpresaId),
    FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Tabela: Prestadores
-- =============================================
CREATE TABLE IF NOT EXISTS Prestadores (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EmpresaId INT NOT NULL,
    RazaoSocial VARCHAR(200) NOT NULL,
    NomeFantasia VARCHAR(200),
    CNPJ VARCHAR(14) NOT NULL,
    InscricaoMunicipal VARCHAR(50) NOT NULL,
    Endereco VARCHAR(200) NOT NULL,
    Cidade VARCHAR(100) NOT NULL,
    UF VARCHAR(2) NOT NULL,
    CEP VARCHAR(8) NOT NULL,
    Telefone VARCHAR(20),
    Email VARCHAR(100),
    CertificadoDigital TEXT,
    SenhaCertificado VARCHAR(500),
    Ambiente INT NOT NULL DEFAULT 1,
    Ativo TINYINT(1) NOT NULL DEFAULT 1,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME,
    INDEX idx_cnpj (CNPJ),
    INDEX idx_inscricao_municipal (InscricaoMunicipal),
    INDEX idx_empresa (EmpresaId),
    FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Tabela: Tomadores
-- =============================================
CREATE TABLE IF NOT EXISTS Tomadores (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TipoPessoa INT NOT NULL,
    CPFCNPJ VARCHAR(14) NOT NULL,
    RazaoSocialNome VARCHAR(200) NOT NULL,
    InscricaoEstadual VARCHAR(50),
    InscricaoMunicipal VARCHAR(50),
    Endereco VARCHAR(200) NOT NULL,
    Numero VARCHAR(20) NOT NULL,
    Complemento VARCHAR(100),
    Bairro VARCHAR(100) NOT NULL,
    Cidade VARCHAR(100) NOT NULL,
    UF VARCHAR(2) NOT NULL,
    CEP VARCHAR(8) NOT NULL,
    Email VARCHAR(100),
    Telefone VARCHAR(20),
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME,
    INDEX idx_cpfcnpj (CPFCNPJ)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Tabela: ConfiguracoesAPI
-- =============================================
CREATE TABLE IF NOT EXISTS ConfiguracoesAPI (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Ambiente INT NOT NULL UNIQUE,
    UrlBase VARCHAR(500) NOT NULL,
    ClientId VARCHAR(200),
    ClientSecret VARCHAR(500),
    Scope VARCHAR(200),
    Timeout INT NOT NULL DEFAULT 30,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Tabela: NotasFiscais
-- =============================================
CREATE TABLE IF NOT EXISTS NotasFiscais (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PrestadorId INT NOT NULL,
    TomadorId INT NOT NULL,
    Numero VARCHAR(50),
    CodigoVerificacao VARCHAR(100),
    Serie VARCHAR(10) NOT NULL DEFAULT '1',
    Competencia DATETIME NOT NULL,
    DataEmissao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataVencimento DATETIME,
    ValorServicos DECIMAL(18,2) NOT NULL,
    ValorDeducoes DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorPis DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorCofins DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorInss DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorIr DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorCsll DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorIss DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorIssRetido DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorLiquido DECIMAL(18,2) NOT NULL,
    Situacao INT NOT NULL DEFAULT 1,
    DiscriminacaoServicos TEXT NOT NULL,
    CodigoMunicipio VARCHAR(7) NOT NULL,
    Observacoes TEXT,
    XML LONGTEXT,
    JSON LONGTEXT,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME,
    INDEX idx_numero (Numero),
    INDEX idx_prestador (PrestadorId),
    INDEX idx_tomador (TomadorId),
    INDEX idx_data_emissao (DataEmissao),
    INDEX idx_situacao (Situacao),
    FOREIGN KEY (PrestadorId) REFERENCES Prestadores(Id) ON DELETE RESTRICT,
    FOREIGN KEY (TomadorId) REFERENCES Tomadores(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Tabela: ItensServico
-- =============================================
CREATE TABLE IF NOT EXISTS ItensServico (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NotaFiscalId INT NOT NULL,
    CodigoServico VARCHAR(20) NOT NULL,
    Discriminacao TEXT NOT NULL,
    Quantidade DECIMAL(10,2) NOT NULL DEFAULT 1,
    ValorUnitario DECIMAL(18,2) NOT NULL,
    ValorTotal DECIMAL(18,2) NOT NULL,
    AliquotaIss DECIMAL(5,2) NOT NULL DEFAULT 0,
    ItemListaServico VARCHAR(10) NOT NULL,
    FOREIGN KEY (NotaFiscalId) REFERENCES NotasFiscais(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- Dados Iniciais (Opcional)
-- =============================================

-- Inserir configurações padrão da API Nacional (Homologação)
INSERT IGNORE INTO ConfiguracoesAPI (Ambiente, UrlBase, Timeout)
VALUES (1, 'https://api-homologacao.nfse.gov.br', 30);

-- Inserir configurações padrão da API Nacional (Produção)
INSERT IGNORE INTO ConfiguracoesAPI (Ambiente, UrlBase, Timeout)
VALUES (2, 'https://api.nfse.gov.br', 30);

-- =============================================
-- Fim do Script
-- =============================================

