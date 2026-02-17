using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFSe2026.API.Models;

[Table("Usuarios")]
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmpresaId { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string SenhaHash { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; } = true;

    public bool EmailValidado { get; set; } = false;

    [StringLength(10)]
    public string? CodigoValidacao { get; set; }

    public DateTime? DataExpiracaoCodigo { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }

    public DateTime? UltimoAcesso { get; set; }

    // Navigation property
    [ForeignKey("EmpresaId")]
    public virtual Empresa Empresa { get; set; } = null!;
}

