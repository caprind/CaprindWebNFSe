namespace NFSe2026.API.Models;

public enum TipoPessoa
{
    Fisica = 1,
    Juridica = 2
}

public enum Ambiente
{
    Homologacao = 1,
    Producao = 2
}

public enum SituacaoNotaFiscal
{
    Rascunho = 1,
    Autorizada = 2,
    Cancelada = 3,
    Rejeitada = 4,
    Enviada = 5 // Nota enviada para SEFAZ, aguardando autorização
}

