-- Script para inserir configuração inicial da API Nacional NFSe
-- Execute este script no banco de dados para criar a configuração de homologação

INSERT INTO ConfiguracoesAPI (Ambiente, UrlBase, ClientId, ClientSecret, Scope, Timeout, DataCriacao)
VALUES (0, 'https://api-homologacao.nfse.gov.br', 'your_client_id', 'your_client_secret', 'nfse', 30, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
    UrlBase = VALUES(UrlBase),
    ClientId = VALUES(ClientId),
    ClientSecret = VALUES(ClientSecret),
    Scope = VALUES(Scope),
    Timeout = VALUES(Timeout),
    DataAtualizacao = UTC_TIMESTAMP();

-- Para produção, adicione também:
-- INSERT INTO ConfiguracoesAPI (Ambiente, UrlBase, ClientId, ClientSecret, Scope, Timeout, DataCriacao)
-- VALUES (1, 'https://api.nfse.gov.br', 'your_production_client_id', 'your_production_client_secret', 'nfse', 30, UTC_TIMESTAMP())
-- ON DUPLICATE KEY UPDATE ...



