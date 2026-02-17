using NFSe2026.API.Models;

namespace NFSe2026.API.DTOs;

public class NotaFiscalDTO
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int TomadorId { get; set; }
    public string? TomadorNome { get; set; } // Nome/Razão Social do tomador
    public string? Numero { get; set; }
    public string? CodigoVerificacao { get; set; }
    public string Serie { get; set; } = string.Empty;
    public DateTime Competencia { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime? DataVencimento { get; set; }
    public decimal ValorServicos { get; set; }
    public decimal ValorDeducoes { get; set; }
    public decimal ValorPis { get; set; }
    public decimal ValorCofins { get; set; }
    public decimal ValorInss { get; set; }
    public decimal ValorIr { get; set; }
    public decimal ValorCsll { get; set; }
    public decimal ValorIss { get; set; }
    public decimal ValorIssRetido { get; set; }
    public decimal ValorLiquido { get; set; }
    public int Situacao { get; set; } // Convertido para int para compatibilidade com front-end
    public string DiscriminacaoServicos { get; set; } = string.Empty;
    public string CodigoMunicipio { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public string? MotivoRejeicao { get; set; } // Motivo/causa da rejeição
    public string? PDFUrl { get; set; } // URL do PDF da nota fiscal (quando disponível)
    public string? XMotivo { get; set; } // Motivo/descrição do status (da consulta de status)
    public List<ItemServicoDTO> ItensServico { get; set; } = new();
}

public class NotaFiscalCreateDTO
{
    // EmpresaId não é necessário, será obtido do token JWT
    public int TomadorId { get; set; } = 0; // 0 = não identificado (tomador opcional)
    public string Serie { get; set; } = "900"; // Padrão 900 conforme exemplo da DANFSe
    public DateTime Competencia { get; set; }
    public DateTime? DataVencimento { get; set; }
    public decimal ValorServicos { get; set; }
    public decimal ValorDeducoes { get; set; } = 0;
    public decimal ValorPis { get; set; } = 0;
    public decimal ValorCofins { get; set; } = 0;
    public decimal ValorInss { get; set; } = 0;
    public decimal ValorIr { get; set; } = 0;
    public decimal ValorCsll { get; set; } = 0;
    public decimal ValorIss { get; set; } = 0;
    public decimal ValorIssRetido { get; set; } = 0;
    public string DiscriminacaoServicos { get; set; } = string.Empty;
    public string CodigoMunicipio { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public List<ItemServicoCreateDTO> ItensServico { get; set; } = new();
}

public class ItemServicoDTO
{
    public int Id { get; set; }
    public int NotaFiscalId { get; set; }
    public string CodigoServico { get; set; } = string.Empty;
    public string Discriminacao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal AliquotaIss { get; set; }
    public string ItemListaServico { get; set; } = string.Empty;
}

public class ItemServicoCreateDTO
{
    public string CodigoServico { get; set; } = string.Empty;
    public string Discriminacao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
    public decimal AliquotaIss { get; set; } = 0;
    public string ItemListaServico { get; set; } = string.Empty;
}

public class NotaFiscalUpdateDTO
{
    public int TomadorId { get; set; } = 0; // 0 = não identificado (tomador opcional)
    public string Serie { get; set; } = "900"; // Padrão 900 conforme exemplo da DANFSe
    public DateTime Competencia { get; set; }
    public DateTime? DataVencimento { get; set; }
    public decimal ValorServicos { get; set; }
    public decimal ValorDeducoes { get; set; } = 0;
    public decimal ValorPis { get; set; } = 0;
    public decimal ValorCofins { get; set; } = 0;
    public decimal ValorInss { get; set; } = 0;
    public decimal ValorIr { get; set; } = 0;
    public decimal ValorCsll { get; set; } = 0;
    public decimal ValorIss { get; set; } = 0;
    public decimal ValorIssRetido { get; set; } = 0;
    public string DiscriminacaoServicos { get; set; } = string.Empty;
    public string CodigoMunicipio { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public List<ItemServicoCreateDTO> ItensServico { get; set; } = new();
}

public class CancelarNotaFiscalDTO
{
    public string Motivo { get; set; } = string.Empty;
}

