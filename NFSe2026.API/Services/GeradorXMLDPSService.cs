using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using Microsoft.Extensions.Logging;

namespace NFSe2026.API.Services;

public class GeradorXMLDPSService : IGeradorXMLDPSService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GeradorXMLDPSService> _logger;

    public GeradorXMLDPSService(ApplicationDbContext context, ILogger<GeradorXMLDPSService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GerarXMLDPSAsync(NotaFiscalCreateDTO notaFiscal, int empresaId)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null)
            throw new Exception("Empresa não encontrada");

        // Tomador é opcional (pode ser "não identificado")
        var tomador = notaFiscal.TomadorId > 0 
            ? await _context.Tomadores.FirstOrDefaultAsync(t => t.Id == notaFiscal.TomadorId && t.EmpresaId == empresaId)
            : null;

        var primeiroItem = notaFiscal.ItensServico.FirstOrDefault();
        if (primeiroItem == null)
            throw new Exception("A nota fiscal deve ter pelo menos um item de serviço");

        // Gera ID da DPS (formato conforme manual ADN)
        var dpsId = $"DPS-{DateTime.UtcNow:yyyyMMddHHmmss}-{empresaId}";
        
        // Série do DPS (padrão 900 conforme exemplo da DANFSe, ou usar série da nota)
        var serieDPS = !string.IsNullOrWhiteSpace(notaFiscal.Serie) && notaFiscal.Serie != "1" 
            ? notaFiscal.Serie 
            : "900";

        // Código do município do tomador (se houver)
        string? codigoMunicipioTomador = null;
        if (tomador != null)
        {
            codigoMunicipioTomador = GetCodigoMunicipioByCidadeUF(tomador.Cidade, tomador.UF) 
                ?? empresa.CodigoMunicipio ?? notaFiscal.CodigoMunicipio;
        }

        // Código de Tributação Nacional - converter ItemListaServico (01.07.01) para formato sem pontos (010701)
        // Ou usar CodigoServico se já estiver no formato correto
        var codigoTributacaoNacional = "010701"; // Padrão
        if (!string.IsNullOrWhiteSpace(primeiroItem.ItemListaServico))
        {
            // Remove pontos e formata para 6 dígitos (01.07.01 -> 010701)
            codigoTributacaoNacional = LimparNBS(primeiroItem.ItemListaServico).PadLeft(6, '0');
        }
        else if (!string.IsNullOrWhiteSpace(primeiroItem.CodigoServico))
        {
            // Remove pontos se houver e formata para 6 dígitos
            codigoTributacaoNacional = primeiroItem.CodigoServico.Replace(".", "").Replace("-", "").Trim().PadLeft(6, '0');
        }

        // Gera o XML conforme estrutura do Manual APIs ADN
        // Namespace obrigatório conforme exemplo do sistema do governo
        XNamespace ns = "http://www.sped.fazenda.gov.br/nfse";
        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "DPS",
                new XAttribute("xmlns", ns),
                new XAttribute("versao", "1.01"),
                new XAttribute("Id", dpsId),
                new XElement(ns + "Serie", serieDPS),
                new XElement(ns + "Competencia", notaFiscal.Competencia.ToString("yyyy-MM")),
                new XElement(ns + "Prestador",
                    new XElement(ns + "CpfCnpj", LimparDocumento(empresa.CNPJ)),
                    new XElement(ns + "InscricaoMunicipal", empresa.InscricaoMunicipal ?? string.Empty),
                    new XElement(ns + "CodigoMunicipio", empresa.CodigoMunicipio ?? notaFiscal.CodigoMunicipio)
                ),
                // Tomador - opcional (pode ser não identificado)
                tomador != null ? new XElement(ns + "Tomador",
                    new XElement(ns + "CpfCnpj", LimparDocumento(tomador.CPFCNPJ)),
                    new XElement(ns + "RazaoSocial", tomador.RazaoSocialNome),
                    new XElement(ns + "Endereco",
                        new XElement(ns + "Logradouro", tomador.Endereco),
                        new XElement(ns + "Numero", tomador.Numero ?? ""),
                        new XElement(ns + "Bairro", tomador.Bairro),
                        new XElement(ns + "CodigoMunicipio", codigoMunicipioTomador ?? ""),
                        new XElement(ns + "UF", tomador.UF),
                        new XElement(ns + "CEP", LimparCEP(tomador.CEP))
                    )
                ) : null,
                new XElement(ns + "Servico",
                    new XElement(ns + "Codigo", codigoTributacaoNacional), // Código de Tributação Nacional (010701)
                    new XElement(ns + "Descricao", 
                        string.IsNullOrWhiteSpace(primeiroItem.Discriminacao) 
                            ? notaFiscal.DiscriminacaoServicos 
                            : primeiroItem.Discriminacao),
                    new XElement(ns + "Aliquota", (primeiroItem.AliquotaIss / 100m).ToString("F4", CultureInfo.InvariantCulture)),
                    new XElement(ns + "Valor",
                        new XElement(ns + "Servico", notaFiscal.ValorServicos.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "DescontoIncondicionado", notaFiscal.ValorDeducoes.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "DescontoCondicionado", "0.00")
                    ),
                    // NBS (Nomenclatura Brasileira de Serviços) / IBS - se disponível
                    !string.IsNullOrWhiteSpace(primeiroItem.ItemListaServico) 
                        ? new XElement(ns + "NBS", LimparNBS(primeiroItem.ItemListaServico))
                        : null,
                    // Impostos
                    new XElement(ns + "Impostos",
                        new XElement(ns + "PIS", notaFiscal.ValorPis.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "COFINS", notaFiscal.ValorCofins.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "CSLL", notaFiscal.ValorCsll.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "IRPJ", notaFiscal.ValorIr.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "ISS", notaFiscal.ValorIss.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "INSS", notaFiscal.ValorInss.ToString("F2", CultureInfo.InvariantCulture))
                    )
                ),
                new XElement(ns + "RegimeEspecialTributacao", empresa.RegimeEspecialTributacao ?? "Nenhum"),
                new XElement(ns + "OptanteSimplesNacional", empresa.OptanteSimplesNacional ? "true" : "false"),
                new XElement(ns + "IncentivoFiscal", empresa.IncentivoFiscal ? "true" : "false")
            )
        );

        // Converte para string usando StringWriter para evitar problemas com namespace
        string xmlString;
        try
        {
            using (var sw = new System.IO.StringWriter())
            {
                xml.Save(sw);
                xmlString = sw.ToString();
            }
            _logger.LogInformation("XML DPS gerado com sucesso. ID: {DpsId}, Série: {Serie}, Tomador: {TemTomador}", 
                dpsId, serieDPS, tomador != null ? "Identificado" : "Não Identificado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter XML para string. Tentando método alternativo...");
            // Tenta método alternativo sem formatação
            xmlString = xml.ToString(SaveOptions.DisableFormatting);
            _logger.LogInformation("XML DPS gerado usando método alternativo. ID: {DpsId}", dpsId);
        }
        return xmlString;
    }

    private string? GetCodigoMunicipioByCidadeUF(string cidade, string uf)
    {
        // Por enquanto retorna null - idealmente deveria buscar de uma tabela de municípios
        // Ou usar uma API externa para buscar o código IBGE do município
        return null;
    }

    private string LimparDocumento(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return string.Empty;
        
        return documento.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
    }

    private string LimparCEP(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return string.Empty;
        
        return cep.Replace("-", "").Replace(".", "").Trim();
    }

    private string LimparNBS(string nbs)
    {
        if (string.IsNullOrWhiteSpace(nbs))
            return string.Empty;
        
        // Remove pontos e hífens do NBS (ex: 01.07.01 -> 010701)
        return nbs.Replace(".", "").Replace("-", "").Trim();
    }
}
