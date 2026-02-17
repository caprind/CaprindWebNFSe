using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFSe2026.API.Models;

[Table("Prestadores")]
public class Prestador
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    [StringLength(200)]
    public string? NomeFantasia { get; set; }

    [Required]
    public int EmpresaId { get; set; }

    [Required]
    [StringLength(14)]
    public string CNPJ { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string InscricaoMunicipal { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Endereco { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    [StringLength(2)]
    public string UF { get; set; } = string.Empty;

    [Required]
    [StringLength(8)]
    public string CEP { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(5000)]
    public string? CertificadoDigital { get; set; } // Base64 ou caminho

    [StringLength(500)]
    public string? SenhaCertificado { get; set; } // Será criptografada

    public Ambiente Ambiente { get; set; } = Ambiente.Homologacao;

    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }

    // Navigation properties
    [ForeignKey("EmpresaId")]
    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<NotaFiscal> NotasFiscais { get; set; } = new List<NotaFiscal>();
}

