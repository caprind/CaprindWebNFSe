using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFSe2026.API.Models;

[Table("ItensServico")]
public class ItemServico
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int NotaFiscalId { get; set; }

    [Required]
    [StringLength(20)]
    public string CodigoServico { get; set; } = string.Empty; // Código da lista de serviços

    [Required]
    [Column(TypeName = "text")]
    public string Discriminacao { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Quantidade { get; set; } = 1;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorUnitario { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal AliquotaIss { get; set; } = 0;

    [Required]
    [StringLength(10)]
    public string ItemListaServico { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("NotaFiscalId")]
    public virtual NotaFiscal NotaFiscal { get; set; } = null!;
}

