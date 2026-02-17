using System.ComponentModel.DataAnnotations;

namespace NFSe2026.Web.Models;

public class NotaFiscalViewModel
{
    public int Id { get; set; }

    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [Display(Name = "Código Verificação")]
    public string? CodigoVerificacao { get; set; }

    [Display(Name = "Série")]
    public string Serie { get; set; } = string.Empty;

    [Display(Name = "Competência")]
    [DataType(DataType.Date)]
    public DateTime Competencia { get; set; }

    [Display(Name = "Data Emissão")]
    [DataType(DataType.DateTime)]
    public DateTime DataEmissao { get; set; }

    [Display(Name = "Valor Total")]
    [DataType(DataType.Currency)]
    public decimal ValorLiquido { get; set; }

    [Display(Name = "Valor dos Serviços")]
    [DataType(DataType.Currency)]
    public decimal ValorServicos { get; set; }

    [Display(Name = "Valor Deduções")]
    [DataType(DataType.Currency)]
    public decimal ValorDeducoes { get; set; }

    [Display(Name = "PIS")]
    [DataType(DataType.Currency)]
    public decimal ValorPis { get; set; }

    [Display(Name = "COFINS")]
    [DataType(DataType.Currency)]
    public decimal ValorCofins { get; set; }

    [Display(Name = "CSLL")]
    [DataType(DataType.Currency)]
    public decimal ValorCsll { get; set; }

    [Display(Name = "IRPJ")]
    [DataType(DataType.Currency)]
    public decimal ValorIr { get; set; }

    [Display(Name = "ISS")]
    [DataType(DataType.Currency)]
    public decimal ValorIss { get; set; }

    [Display(Name = "INSS")]
    [DataType(DataType.Currency)]
    public decimal ValorInss { get; set; }

    [Display(Name = "Situação")]
    public int Situacao { get; set; }

    [Display(Name = "Empresa (Prestador)")]
    public int EmpresaId { get; set; }

    [Display(Name = "Tomador")]
    public int TomadorId { get; set; }

    [Display(Name = "Nome do Tomador")]
    public string? TomadorNome { get; set; }

    [Display(Name = "Discriminação dos Serviços")]
    public string DiscriminacaoServicos { get; set; } = string.Empty;

    [Display(Name = "Código do Município")]
    public string? CodigoMunicipio { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    [Display(Name = "Motivo da Rejeição")]
    public string? MotivoRejeicao { get; set; }

    [Display(Name = "URL do PDF")]
    public string? PDFUrl { get; set; }

    [Display(Name = "Motivo do Status")]
    public string? XMotivo { get; set; }

    public List<ItemServicoViewModel> ItensServico { get; set; } = new();
}

public class ItemServicoViewModel
{
    public int Id { get; set; }
    public string CodigoServico { get; set; } = string.Empty;
    public string Discriminacao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal AliquotaIss { get; set; }
    public string ItemListaServico { get; set; } = string.Empty;
}

public class NotaFiscalCreateViewModel
{
    // EmpresaId não é necessário - será obtido do token JWT (empresa logada é o prestador)

    [Required(ErrorMessage = "O tomador é obrigatório")]
    [Display(Name = "Tomador")]
    public int TomadorId { get; set; }

    [Display(Name = "Série")]
    public string Serie { get; set; } = "1";

    [Required(ErrorMessage = "A competência é obrigatória")]
    [Display(Name = "Competência")]
    [DataType(DataType.Date)]
    public DateTime Competencia { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "O valor dos serviços é obrigatório")]
    [Display(Name = "Valor dos Serviços")]
    [DataType(DataType.Currency)]
    public decimal ValorServicos { get; set; }

    [Display(Name = "Valor Deduções")]
    [DataType(DataType.Currency)]
    public decimal ValorDeducoes { get; set; } = 0;

    [Display(Name = "PIS")]
    [DataType(DataType.Currency)]
    public decimal ValorPis { get; set; } = 0;

    [Display(Name = "COFINS")]
    [DataType(DataType.Currency)]
    public decimal ValorCofins { get; set; } = 0;

    [Display(Name = "CSLL")]
    [DataType(DataType.Currency)]
    public decimal ValorCsll { get; set; } = 0;

    [Display(Name = "IRPJ")]
    [DataType(DataType.Currency)]
    public decimal ValorIr { get; set; } = 0;

    [Display(Name = "ISS")]
    [DataType(DataType.Currency)]
    public decimal ValorIss { get; set; } = 0;

    [Display(Name = "INSS")]
    [DataType(DataType.Currency)]
    public decimal ValorInss { get; set; } = 0;

    [Required(ErrorMessage = "A discriminação dos serviços é obrigatória")]
    [Display(Name = "Discriminação dos Serviços")]
    public string DiscriminacaoServicos { get; set; } = string.Empty;

    [Display(Name = "Código de Tributação Nacional")]
    public string CodigoServico { get; set; } = "010701"; // Código padrão

    [Display(Name = "Código IBS (Item Lista Serviço)")]
    public string ItemListaServico { get; set; } = "111032200"; // Código padrão

    [Display(Name = "Código do Município (opcional - será usado o da empresa se não informado)")]
    public string? CodigoMunicipio { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    // Para dropdowns
    public List<TomadorViewModel> Tomadores { get; set; } = new();
}

