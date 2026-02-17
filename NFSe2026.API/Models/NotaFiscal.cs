using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFSe2026.API.Models;

[Table("NotasFiscais")]
public class NotaFiscal
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmpresaId { get; set; }

    // Tomador é opcional (0 = não identificado)
    public int TomadorId { get; set; } = 0;

    [StringLength(50)]
    public string? Numero { get; set; } // Número gerado pela API

    [StringLength(100)]
    public string? CodigoVerificacao { get; set; }

    [StringLength(10)]
    public string Serie { get; set; } = "900"; // Padrão 900 conforme exemplo da DANFSe

    [Required]
    public DateTime Competencia { get; set; }

    [Required]
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;

    public DateTime? DataVencimento { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorServicos { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDeducoes { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorPis { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCofins { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorInss { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorIr { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCsll { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorIss { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorIssRetido { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorLiquido { get; set; }

    [Required]
    public SituacaoNotaFiscal Situacao { get; set; } = SituacaoNotaFiscal.Rascunho;

    [Required]
    [Column(TypeName = "text")]
    public string DiscriminacaoServicos { get; set; } = string.Empty;

    [Required]
    [StringLength(7)]
    public string CodigoMunicipio { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string? Observacoes { get; set; }

    [Column(TypeName = "longtext")]
    public string? XML { get; set; } // XML da nota

    [Column(TypeName = "longtext")]
    public string? JSON { get; set; } // JSON da nota

    [Column(TypeName = "text")]
    public string? PDFUrl { get; set; } // URL do PDF da nota fiscal (quando disponível)

    [Column(TypeName = "text")]
    public string? MotivoRejeicao { get; set; } // Motivo/causa da rejeição

    [StringLength(100)]
    public string? NsNRec { get; set; } // Número do protocolo de recebimento (usado para consultar status)

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }

    // Navigation properties
    [ForeignKey("EmpresaId")]
    public virtual Empresa Empresa { get; set; } = null!;

    [ForeignKey("TomadorId")]
    public virtual Tomador? Tomador { get; set; } // Tomador opcional (pode ser null se TomadorId = 0)

    public virtual ICollection<ItemServico> ItensServico { get; set; } = new List<ItemServico>();
}

