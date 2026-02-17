using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFSe2026.API.Models;

[Table("Tomadores")]
public class Tomador
{
    [Key]
    public int Id { get; set; }

    [Required]
    public TipoPessoa TipoPessoa { get; set; }

    [Required]
    [StringLength(14)]
    public string CPFCNPJ { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string RazaoSocialNome { get; set; } = string.Empty;

    [StringLength(50)]
    public string? InscricaoEstadual { get; set; }

    [StringLength(50)]
    public string? InscricaoMunicipal { get; set; }

    [Required]
    [StringLength(200)]
    public string Endereco { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Complemento { get; set; }

    [Required]
    [StringLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    [StringLength(2)]
    public string UF { get; set; } = string.Empty;

    [Required]
    [StringLength(8)]
    public string CEP { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    [Required]
    public int EmpresaId { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }

    // Navigation properties
    [ForeignKey("EmpresaId")]
    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<NotaFiscal> NotasFiscais { get; set; } = new List<NotaFiscal>();
}

