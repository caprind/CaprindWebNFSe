using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFSe2026.API.Models;

[Table("Empresas")]
public class Empresa
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(14)]
    public string CNPJ { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    [StringLength(200)]
    public string? NomeFantasia { get; set; }

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

    [StringLength(7)]
    public string? CodigoMunicipio { get; set; } // Código IBGE do município (opcional, mas recomendado)

    [StringLength(50)]
    public string? RegimeEspecialTributacao { get; set; } = "Nenhum"; // Regime especial de tributação

    public bool OptanteSimplesNacional { get; set; } = true; // Indica se a empresa é optante do Simples Nacional

    public bool IncentivoFiscal { get; set; } = false; // Indica se há incentivo fiscal

    [Required]
    [StringLength(8)]
    public string CEP { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [Column(TypeName = "longtext")]
    public string? CertificadoDigital { get; set; } // Base64 do certificado .pfx/.p12 (pode ser grande)

    [StringLength(500)]
    public string? SenhaCertificado { get; set; } // Será criptografada

    public DateTime? DataVencimentoCertificado { get; set; } // Data de vencimento do certificado digital

    [StringLength(500)]
    public string? ClientId { get; set; } // ClientId da API Nacional NFSe (será criptografado)

    [StringLength(500)]
    public string? ClientSecret { get; set; } // ClientSecret da API Nacional NFSe (será criptografado)

    /// <summary>
    /// Provedor de NFSe a ser utilizado para emissão (Nacional ou NS Tecnologia)
    /// </summary>
    public ProvedorNFSe? ProvedorNFSe { get; set; }

    public Ambiente Ambiente { get; set; } = Ambiente.Homologacao;

    [StringLength(50)]
    public string? SituacaoCadastral { get; set; }

    [StringLength(50)]
    public string? Porte { get; set; }

    [StringLength(200)]
    public string? NaturezaJuridica { get; set; }

    [Column(TypeName = "longtext")]
    public string? Logotipo { get; set; } // Base64 da imagem do logotipo

    public DateTime? DataAbertura { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }

    // Navigation properties
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<Tomador> Tomadores { get; set; } = new List<Tomador>();
    public virtual ICollection<NotaFiscal> NotasFiscais { get; set; } = new List<NotaFiscal>();
}

