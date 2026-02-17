using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFSe2026.API.Models;

[Table("ConfiguracoesAPI")]
public class ConfiguracaoAPI
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Ambiente Ambiente { get; set; }

    [Required]
    [StringLength(500)]
    public string UrlBase { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ClientId { get; set; }

    [StringLength(500)]
    public string? ClientSecret { get; set; } // Será criptografada

    [StringLength(200)]
    public string? Scope { get; set; }

    public int Timeout { get; set; } = 30; // em segundos

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }
}

