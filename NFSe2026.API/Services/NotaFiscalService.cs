using System.IO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;

namespace NFSe2026.API.Services;

public class NotaFiscalService : INotaFiscalService
{
    private readonly ApplicationDbContext _context;
    private readonly IProvedorNFSeFactory _provedorFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<NotaFiscalService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _pastaPDF;
    private readonly IEmailService _emailService;

    public NotaFiscalService(
        ApplicationDbContext context,
        IProvedorNFSeFactory provedorFactory,
        IMapper mapper,
        ILogger<NotaFiscalService> logger,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _context = context;
        _provedorFactory = provedorFactory;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
        
        // Configuração da pasta de documentos PDF
        _pastaPDF = _configuration["Documentos:PastaPDF"] ?? "Documentos/Nota Fiscal/PDF";
        
        // Cria a pasta se não existir
        try
        {
            var pastaCompleta = Path.IsPathRooted(_pastaPDF) ? _pastaPDF : Path.Combine(Directory.GetCurrentDirectory(), _pastaPDF);
            if (!Directory.Exists(pastaCompleta))
            {
                Directory.CreateDirectory(pastaCompleta);
                _logger.LogInformation("Pasta de documentos PDF criada: {Pasta}", pastaCompleta);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar a pasta de documentos PDF: {Pasta}", _pastaPDF);
        }
    }

    public async Task<NotaFiscalDTO> CriarNotaFiscalAsync(NotaFiscalCreateDTO notaFiscalDto, int empresaId)
    {
        // Validações
        await ValidarDadosAsync(notaFiscalDto, empresaId);

        // Busca a empresa para obter o código do município
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null)
        {
            throw new InvalidOperationException("Empresa não encontrada");
        }

        // Cria a nota fiscal no banco de dados com status Rascunho
        _logger.LogInformation("📝 Criando nota fiscal - TomadorId recebido no DTO: {TomadorId}", notaFiscalDto.TomadorId);
        var notaFiscal = _mapper.Map<NotaFiscal>(notaFiscalDto);
        notaFiscal.EmpresaId = empresaId; // Define a empresa logada como prestador
        _logger.LogInformation("📝 TomadorId após mapeamento: {TomadorId}", notaFiscal.TomadorId);
        
        // Preenche o código do município da empresa (prestador) se não foi informado
        if (string.IsNullOrWhiteSpace(notaFiscal.CodigoMunicipio))
        {
            if (!string.IsNullOrWhiteSpace(empresa.CodigoMunicipio))
            {
                notaFiscal.CodigoMunicipio = empresa.CodigoMunicipio;
            }
            else
            {
                // Se nem a empresa tem código, lança erro
                throw new InvalidOperationException("Código do município é obrigatório. Configure o código do município na empresa ou informe na nota fiscal.");
            }
        }
        
        notaFiscal.Situacao = SituacaoNotaFiscal.Rascunho;
        notaFiscal.DataEmissao = DateTime.Now;
        notaFiscal.ValorLiquido = CalcularValorLiquido(notaFiscal);
        
        // Gera número sequencial da nota fiscal baseado na última nota emitida
        var proximoNumero = await GerarProximoNumeroNotaFiscalAsync(empresaId);
        notaFiscal.Numero = proximoNumero;
        _logger.LogInformation("📝 Número da nota fiscal gerado automaticamente: {Numero}", notaFiscal.Numero);

        // Salva os itens de serviço
        foreach (var itemDto in notaFiscalDto.ItensServico)
        {
            var item = _mapper.Map<ItemServico>(itemDto);
            item.NotaFiscalId = 0; // Será definido após salvar a nota
            item.ValorTotal = item.Quantidade * item.ValorUnitario;
            notaFiscal.ItensServico.Add(item);
        }

        _context.NotasFiscais.Add(notaFiscal);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Id} criada com sucesso (status: Rascunho)", notaFiscal.Id);

        return _mapper.Map<NotaFiscalDTO>(notaFiscal);
    }

    /// <summary>
    /// Gera o próximo número sequencial da nota fiscal baseado na última nota criada para a empresa
    /// </summary>
    private async Task<string> GerarProximoNumeroNotaFiscalAsync(int empresaId)
    {
        try
        {
            // Busca todas as notas fiscais da empresa que têm número preenchido
            var notasComNumero = await _context.NotasFiscais
                .Where(n => n.EmpresaId == empresaId && !string.IsNullOrWhiteSpace(n.Numero))
                .Select(n => n.Numero!)
                .ToListAsync();
            
            int maiorNumero = 0;
            
            // Percorre todas as notas e encontra o maior número (convertido para inteiro)
            foreach (var numeroStr in notasComNumero)
            {
                // Remove formatação (zeros à esquerda, espaços, etc) e tenta converter para inteiro
                var numeroLimpo = numeroStr.Trim().TrimStart('0');
                if (string.IsNullOrWhiteSpace(numeroLimpo))
                {
                    // Se após remover zeros só sobrou string vazia, o número era só zeros
                    numeroLimpo = "0";
                }
                
                if (int.TryParse(numeroLimpo, out var numeroInt))
                {
                    if (numeroInt > maiorNumero)
                    {
                        maiorNumero = numeroInt;
                    }
                }
            }
            
            int proximoNumero = maiorNumero + 1;
            
            if (maiorNumero > 0)
            {
                _logger.LogInformation("Maior número de nota fiscal encontrado: {MaiorNumero}, próximo número: {ProximoNumero}", 
                    maiorNumero, proximoNumero);
            }
            else
            {
                _logger.LogInformation("Nenhuma nota fiscal anterior encontrada com número. Iniciando com número 1.");
            }
            
            // Formata o número com 6 dígitos (ex: 000001, 000002, etc)
            return proximoNumero.ToString("D6");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar próximo número de nota fiscal. Usando número padrão 1.");
            return "000001";
        }
    }

    public async Task<NotaFiscalDTO> EmitirNotaFiscalAsync(int id, int empresaId)
    {
        // Busca a nota fiscal
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.ItensServico)
            .Include(n => n.Empresa)
            .Include(n => n.Tomador)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();

        if (notaFiscal == null)
        {
            throw new InvalidOperationException("Nota fiscal não encontrada");
        }

        // Verifica se a nota já foi emitida
        if (notaFiscal.Situacao != SituacaoNotaFiscal.Rascunho)
        {
            throw new InvalidOperationException($"Nota fiscal já foi processada. Situação atual: {notaFiscal.Situacao}");
        }

        // Converte NotaFiscal para NotaFiscalCreateDTO para enviar para a API Nacional
        _logger.LogInformation("🔄 Convertendo NotaFiscal para NotaFiscalCreateDTO - TomadorId no banco: {TomadorId}", notaFiscal.TomadorId);
        var notaFiscalDto = new NotaFiscalCreateDTO
        {
            TomadorId = notaFiscal.TomadorId,
            Serie = notaFiscal.Serie,
            Competencia = notaFiscal.Competencia,
            DataVencimento = notaFiscal.DataVencimento,
            ValorServicos = notaFiscal.ValorServicos,
            ValorDeducoes = notaFiscal.ValorDeducoes,
            ValorPis = notaFiscal.ValorPis,
            ValorCofins = notaFiscal.ValorCofins,
            ValorInss = notaFiscal.ValorInss,
            ValorIr = notaFiscal.ValorIr,
            ValorCsll = notaFiscal.ValorCsll,
            ValorIss = notaFiscal.ValorIss,
            ValorIssRetido = notaFiscal.ValorIssRetido,
            DiscriminacaoServicos = notaFiscal.DiscriminacaoServicos,
            CodigoMunicipio = notaFiscal.CodigoMunicipio,
            Observacoes = notaFiscal.Observacoes,
            ItensServico = notaFiscal.ItensServico.Select(item => new ItemServicoCreateDTO
            {
                CodigoServico = item.CodigoServico,
                Discriminacao = item.Discriminacao,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario,
                AliquotaIss = item.AliquotaIss,
                ItemListaServico = item.ItemListaServico
            }).ToList()
        };

        try
        {
            // Obtém o provedor configurado para a empresa
            var provedor = await _provedorFactory.ObterProvedorAsync(empresaId);
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
            _logger.LogInformation("🚀 PASSO 1 - ENVIO/EMISSÃO DA NOTA FISCAL");
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
            _logger.LogInformation("Emitindo nota fiscal {Id} usando provedor: {Provedor}", notaFiscal.Id, provedor.NomeProvedor);
            
            // Emite no provedor configurado (passa o ID da nota fiscal para usar como nDPS e o número para logs)
            var notaEmitida = await provedor.EmitirNotaFiscalAsync(notaFiscalDto, empresaId, notaFiscal.Id, notaFiscal.Numero);

            // Atualiza a nota com os dados retornados pela API
            _logger.LogInformation("✅ PASSO 1 CONCLUÍDO - Dados retornados pela API:");
            _logger.LogInformation("   - Numero: {Numero}", notaEmitida.Numero ?? "null");
            _logger.LogInformation("   - CodigoVerificacao: {CodigoVerificacao}", notaEmitida.CodigoVerificacao ?? "null");
            _logger.LogInformation("   - Situacao: {Situacao} (valor: {Valor})", notaEmitida.Situacao, (int)notaEmitida.Situacao);
            _logger.LogInformation("   - NsNRec: {NsNRec}", notaEmitida.NsNRec ?? "null");
            
            // Formata o número da nota com 6 dígitos (ex: 000006)
            if (!string.IsNullOrWhiteSpace(notaEmitida.Numero) && int.TryParse(notaEmitida.Numero, out var numeroEmitido))
            {
                notaFiscal.Numero = numeroEmitido.ToString("D6");
                _logger.LogInformation("Número da nota formatado: {Numero}", notaFiscal.Numero);
            }
            else
            {
                notaFiscal.Numero = notaEmitida.Numero;
            }
            notaFiscal.CodigoVerificacao = notaEmitida.CodigoVerificacao;
            // Quando status é 200, significa "NFSe enviado para Sefaz" - usa status "Enviada"
            // Se a situação retornada for 200, marca como "Enviada" (aguardando autorização)
            if (notaEmitida.Situacao == 200)
            {
                _logger.LogInformation("Status 200 recebido (NFSe enviado para Sefaz). Marcando como 'Enviada' (aguardando autorização).");
                notaFiscal.Situacao = SituacaoNotaFiscal.Enviada;
            }
            else
            {
                // Tenta converter apenas se for um valor válido do enum (1, 2, 3, 4, 5)
                if (Enum.IsDefined(typeof(SituacaoNotaFiscal), notaEmitida.Situacao))
                {
                    notaFiscal.Situacao = (SituacaoNotaFiscal)notaEmitida.Situacao;
                }
                else
                {
                    _logger.LogWarning("Situação inválida recebida: {Situacao}. Mantendo como Rascunho.", notaEmitida.Situacao);
                    notaFiscal.Situacao = SituacaoNotaFiscal.Rascunho;
                }
            }
            notaFiscal.XML = notaEmitida.XML;
            notaFiscal.JSON = notaEmitida.JSON;
            notaFiscal.PDFUrl = notaEmitida.PDFUrl;
            notaFiscal.NsNRec = notaEmitida.NsNRec;
            notaFiscal.DataAtualizacao = DateTime.UtcNow;

            _logger.LogInformation("Atualizando nota fiscal {Id} no banco de dados...", notaFiscal.Id);

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Nota fiscal {Id} - {Numero} salva com sucesso. Situacao: {Situacao} (valor: {Valor}), NsNRec: {NsNRec}", 
                notaFiscal.Id, notaFiscal.Numero ?? "null", notaFiscal.Situacao, (int)notaFiscal.Situacao, notaFiscal.NsNRec ?? "null");
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");

            // PASSO 2 e 3 - Consultar status e fazer download do PDF se temos nsNRec
            // Executa mesmo que a nota ainda não esteja autorizada (status 200 = enviado para Sefaz)
            // O PASSO 2 consulta o status para verificar se já foi autorizado e obter as chaves
            if (!string.IsNullOrWhiteSpace(notaFiscal.NsNRec))
            {
                try
                {
                    // Busca CNPJ da empresa
                    var empresa = await _context.Empresas
                        .Where(e => e.Id == empresaId)
                        .Select(e => new { e.CNPJ })
                        .FirstOrDefaultAsync();

                    if (empresa != null && !string.IsNullOrWhiteSpace(empresa.CNPJ))
                    {
                        // PASSO 2 - Consulta o status para obter as chaves chDPS e chNFSe
                        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                        _logger.LogInformation("🔍 PASSO 2 - CONSULTA STATUS DE PROCESSAMENTO");
                        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                        _logger.LogInformation("Consultando status da nota fiscal {Id} - NsNRec: {NsNRec}, CNPJ: {CNPJ}", 
                            notaFiscal.Id, notaFiscal.NsNRec, empresa.CNPJ);
                        
                        var statusResult = await provedor.ConsultarStatusAsync(
                            notaFiscal.NsNRec,
                            empresa.CNPJ,
                            empresaId,
                            notaFiscal.Id, // Passa o ID da nota fiscal para os logs
                            notaFiscal.Numero); // Passa o número da nota para os logs
                        
                        _logger.LogInformation("Resultado da consulta de status - statusResult: {StatusResult}, ChDPS: {ChDPS}, ChNFSe: {ChNFSe}", 
                            statusResult != null ? "não null" : "null", 
                            statusResult?.ChDPS ?? "null", 
                            statusResult?.ChNFSe ?? "null");
                        
                        // Se chNFSe é null, a nota ainda não foi autorizada (pode estar em processamento ou com erro)
                        if (statusResult != null && string.IsNullOrWhiteSpace(statusResult.ChNFSe))
                        {
                            _logger.LogInformation("ℹ️ Nota fiscal {Id} ainda não autorizada. chNFSe não disponível. A nota está aguardando processamento pela SEFAZ.", notaFiscal.Id);
                        }
                        
                        if (statusResult != null && !string.IsNullOrWhiteSpace(statusResult.ChDPS) && !string.IsNullOrWhiteSpace(statusResult.ChNFSe))
                        {
                            _logger.LogInformation("✅ PASSO 2 CONCLUÍDO - Chaves obtidas com sucesso:");
                            _logger.LogInformation("   - chDPS: {ChDPS}", statusResult.ChDPS);
                            _logger.LogInformation("   - chNFSe: {ChNFSe}", statusResult.ChNFSe);
                            _logger.LogInformation("   - Situacao (cStat): {Situacao}", statusResult.Situacao);
                            _logger.LogInformation("═══════════════════════════════════════════════════════════════");

                            // Se a consulta de status retornou cStat=100 (Autorizado), atualiza a situação da nota fiscal
                            // cStat = 100 significa "Autorizado o Uso do Documento"
                            if (statusResult.Situacao == 100)
                            {
                                _logger.LogInformation("🔄 Atualizando situação da nota fiscal para Autorizada (cStat=100)");
                                notaFiscal.Situacao = SituacaoNotaFiscal.Autorizada;
                                
                                // Atualiza o Numero se disponível na resposta (formata com 6 dígitos)
                                if (!string.IsNullOrWhiteSpace(statusResult.Numero))
                                {
                                    if (int.TryParse(statusResult.Numero, out var numero))
                                    {
                                        notaFiscal.Numero = numero.ToString("D6");
                                        _logger.LogInformation("Número da nota atualizado e formatado: {Numero}", notaFiscal.Numero);
                                    }
                                    else
                                    {
                                        notaFiscal.Numero = statusResult.Numero;
                                        _logger.LogInformation("Número da nota atualizado: {Numero}", statusResult.Numero);
                                    }
                                }
                                
                                // Atualiza o CodigoVerificacao se disponível na resposta
                                if (!string.IsNullOrWhiteSpace(statusResult.CodigoVerificacao))
                                {
                                    notaFiscal.CodigoVerificacao = statusResult.CodigoVerificacao;
                                    _logger.LogInformation("Código de verificação atualizado: {CodigoVerificacao}", statusResult.CodigoVerificacao);
                                }
                                
                                // Atualiza o XML se disponível na resposta
                                if (!string.IsNullOrWhiteSpace(statusResult.XML))
                                {
                                    notaFiscal.XML = statusResult.XML;
                                    _logger.LogInformation("XML atualizado da consulta de status");
                                }
                                
                                // Atualiza o JSON se disponível na resposta
                                if (!string.IsNullOrWhiteSpace(statusResult.JSON))
                                {
                                    notaFiscal.JSON = statusResult.JSON;
                                }
                                
                                notaFiscal.DataAtualizacao = DateTime.UtcNow;
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("✅ Situação da nota fiscal atualizada para Autorizada");
                            }

                            // PASSO 3 - Download do PDF usando as chaves obtidas
                            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                            _logger.LogInformation("📥 PASSO 3 - DOWNLOAD DO PDF");
                            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                            _logger.LogInformation("Fazendo download do PDF da nota fiscal {Id} usando chDPS e chNFSe", notaFiscal.Id);
                            
                            byte[]? pdfBytes;
                            // Se for NS Tecnologia, usa a sobrecarga com número da nota para logs
                            if (provedor is NSTecnologiaAPIService nsTecnologia)
                            {
                                pdfBytes = await nsTecnologia.DownloadPDFAsync(statusResult.ChDPS, statusResult.ChNFSe, empresa.CNPJ, empresaId, notaFiscal.Numero, notaFiscal.Id);
                            }
                            else
                            {
                                pdfBytes = await provedor.DownloadPDFAsync(statusResult.ChDPS, statusResult.ChNFSe, empresa.CNPJ, empresaId);
                            }
                            
                            if (pdfBytes != null && pdfBytes.Length > 0)
                            {
                                _logger.LogInformation("✅ PASSO 3 CONCLUÍDO - PDF baixado com sucesso. Tamanho: {Tamanho} bytes", pdfBytes.Length);
                                
                                // Salva o PDF na pasta Documentos/Nota Fiscal/PDF/{CNPJ}
                                try
                                {
                                    var pastaBase = Path.IsPathRooted(_pastaPDF) ? _pastaPDF : Path.Combine(Directory.GetCurrentDirectory(), _pastaPDF);
                                    
                                    // Remove formatação do CNPJ para usar como nome da pasta
                                    var cnpjEmpresa = empresa.CNPJ?.Replace(".", "").Replace("/", "").Replace("-", "").Trim() ?? "SEM_CNPJ";
                                    var pastaCompleta = Path.Combine(pastaBase, cnpjEmpresa);
                                    
                                    // Cria a pasta se não existir
                                    if (!Directory.Exists(pastaCompleta))
                                    {
                                        Directory.CreateDirectory(pastaCompleta);
                                        _logger.LogInformation("Pasta de documentos PDF criada: {PastaCompleta}", pastaCompleta);
                                    }
                                    
                                    // Formata o número da nota com 6 dígitos (ex: 000006)
                                    var numeroFormatado = "000000";
                                    if (!string.IsNullOrWhiteSpace(notaFiscal.Numero) && int.TryParse(notaFiscal.Numero, out var numero))
                                    {
                                        numeroFormatado = numero.ToString("D6");
                                    }
                                    
                                    // Obtém o nome do tomador
                                    var nomeTomador = "Sem Tomador";
                                    if (notaFiscal.Tomador != null && !string.IsNullOrWhiteSpace(notaFiscal.Tomador.RazaoSocialNome))
                                    {
                                        nomeTomador = notaFiscal.Tomador.RazaoSocialNome;
                                    }
                                    
                                    // Remove caracteres inválidos do nome do arquivo
                                    var caracteresInvalidos = Path.GetInvalidFileNameChars();
                                    foreach (var c in caracteresInvalidos)
                                    {
                                        nomeTomador = nomeTomador.Replace(c, '_');
                                    }
                                    
                                    // Nome do arquivo: {NumeroFormatado} - {NomeTomador}.pdf
                                    var nomeArquivo = $"{numeroFormatado} - {nomeTomador}.pdf";
                                    
                                    var caminhoCompleto = Path.Combine(pastaCompleta, nomeArquivo);
                                    
                                    await File.WriteAllBytesAsync(caminhoCompleto, pdfBytes);
                                    
                                    // Atualiza o caminho do PDF na nota fiscal
                                    notaFiscal.PDFUrl = caminhoCompleto;
                                    
                                    // Formata e salva o número da nota com 6 dígitos
                                    if (!string.IsNullOrWhiteSpace(notaFiscal.Numero) && int.TryParse(notaFiscal.Numero, out var numeroNota))
                                    {
                                        notaFiscal.Numero = numeroNota.ToString("D6");
                                    }
                                    
                                    await _context.SaveChangesAsync();
                                    
                                    _logger.LogInformation("✅ PDF salvo com sucesso: {Caminho}", caminhoCompleto);
                                    _logger.LogInformation("✅ Número da nota formatado: {Numero}", notaFiscal.Numero);
                                    _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                                    
                                    // PASSO 4 - Enviar PDF por email automaticamente se a nota estiver autorizada e o tomador tiver email
                                    if (statusResult.Situacao == 100 && notaFiscal.Situacao == SituacaoNotaFiscal.Autorizada)
                                    {
                                        // Recarrega o tomador para garantir que temos os dados atualizados
                                        await _context.Entry(notaFiscal).Reference(n => n.Tomador).LoadAsync();
                                        
                                        if (notaFiscal.Tomador != null && !string.IsNullOrWhiteSpace(notaFiscal.Tomador.Email))
                                        {
                                            try
                                            {
                                                _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                                                _logger.LogInformation("📧 PASSO 4 - ENVIO AUTOMÁTICO DE EMAIL");
                                                _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                                                _logger.LogInformation("Enviando PDF por email automaticamente para: {Email}", notaFiscal.Tomador.Email);
                                                
                                                var numeroNotaEmail = notaFiscal.Numero ?? notaFiscal.Id.ToString();
                                                await _emailService.EnviarPDFPorEmailAsync(
                                                    notaFiscal.Tomador.Email,
                                                    notaFiscal.Tomador.RazaoSocialNome,
                                                    numeroNotaEmail,
                                                    pdfBytes,
                                                    nomeArquivo
                                                );
                                                
                                                _logger.LogInformation("✅ Email enviado com sucesso para {Email}", notaFiscal.Tomador.Email);
                                                _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                                            }
                                            catch (Exception exEmail)
                                            {
                                                _logger.LogError(exEmail, "Erro ao enviar PDF por email automaticamente para {Email}. O PDF foi salvo com sucesso.", notaFiscal.Tomador.Email);
                                                // Não interrompe o fluxo, apenas loga o erro
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogInformation("ℹ️ Tomador não possui email cadastrado. Email não será enviado automaticamente.");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Erro ao salvar PDF da nota fiscal {Id}", notaFiscal.Id);
                                    // Não interrompe o fluxo, apenas loga o erro
                                }
                            }
                            else
                            {
                                _logger.LogWarning("❌ PASSO 3 FALHOU - PDF não foi retornado ou está vazio");
                                _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("❌ PASSO 2 FALHOU - Não foi possível obter as chaves chDPS e chNFSe da consulta de status.");
                            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("CNPJ da empresa não encontrado. Pulando PASSO 2 e 3.");
                    }
                }
                catch (Exception ex)
                {
                    // Log do erro mas não interrompe o fluxo - a nota já foi emitida com sucesso
                    _logger.LogError(ex, "Erro ao executar PASSO 2 ou 3 (consulta status/download PDF) para nota fiscal {Id}. Nota foi emitida com sucesso.", notaFiscal.Id);
                }
            }
            else
            {
                _logger.LogInformation("PASSO 2 e 3 não executados - NsNRec não disponível. Situacao: {Situacao} (valor: {Valor}), NsNRec: {NsNRec}", 
                    notaFiscal.Situacao, (int)notaFiscal.Situacao, notaFiscal.NsNRec ?? "null");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal {Id} na API Nacional", id);
            notaFiscal.Situacao = SituacaoNotaFiscal.Rejeitada;
            notaFiscal.MotivoRejeicao = ex.Message; // Armazena o motivo da rejeição
            notaFiscal.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            throw;
        }

        return _mapper.Map<NotaFiscalDTO>(notaFiscal);
    }

    public async Task<NotaFiscalDTO> AtualizarNotaFiscalAsync(int id, NotaFiscalUpdateDTO notaFiscalDto, int empresaId)
    {
        // Busca a nota fiscal
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.ItensServico)
            .Include(n => n.Tomador)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();

        if (notaFiscal == null)
        {
            throw new InvalidOperationException("Nota fiscal não encontrada");
        }

        // Só permite editar notas em Rascunho
        if (notaFiscal.Situacao != SituacaoNotaFiscal.Rascunho)
        {
            throw new InvalidOperationException($"Não é possível editar uma nota fiscal com situação {notaFiscal.Situacao}. Apenas notas em Rascunho podem ser editadas.");
        }

        // Validações
        // Tomador é opcional - só valida se TomadorId > 0
        if (notaFiscalDto.TomadorId > 0)
        {
            var tomador = await _context.Tomadores
                .Where(t => t.Id == notaFiscalDto.TomadorId && t.EmpresaId == empresaId)
                .FirstOrDefaultAsync();

            if (tomador == null)
            {
                throw new InvalidOperationException("Tomador não encontrado");
            }
        }

        if (notaFiscalDto.ItensServico == null || !notaFiscalDto.ItensServico.Any())
        {
            throw new InvalidOperationException("A nota fiscal deve ter pelo menos um item de serviço");
        }

        // Valida código do município
        if (string.IsNullOrWhiteSpace(notaFiscalDto.CodigoMunicipio))
        {
            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa != null && !string.IsNullOrWhiteSpace(empresa.CodigoMunicipio))
            {
                notaFiscalDto.CodigoMunicipio = empresa.CodigoMunicipio;
            }
            else
            {
                throw new InvalidOperationException("Código do município é obrigatório. Configure o código do município na empresa ou informe na nota fiscal.");
            }
        }

        // Atualiza propriedades
        _logger.LogInformation("📝 Atualizando nota fiscal - TomadorId recebido no DTO: {TomadorId}, TomadorId atual no banco: {TomadorIdAtual}", 
            notaFiscalDto.TomadorId, notaFiscal.TomadorId);
        notaFiscal.TomadorId = notaFiscalDto.TomadorId;
        _logger.LogInformation("📝 TomadorId após atualização: {TomadorId}", notaFiscal.TomadorId);
        notaFiscal.Serie = notaFiscalDto.Serie;
        notaFiscal.Competencia = notaFiscalDto.Competencia;
        notaFiscal.DataVencimento = notaFiscalDto.DataVencimento;
        notaFiscal.ValorServicos = notaFiscalDto.ValorServicos;
        notaFiscal.ValorDeducoes = notaFiscalDto.ValorDeducoes;
        notaFiscal.ValorPis = notaFiscalDto.ValorPis;
        notaFiscal.ValorCofins = notaFiscalDto.ValorCofins;
        notaFiscal.ValorInss = notaFiscalDto.ValorInss;
        notaFiscal.ValorIr = notaFiscalDto.ValorIr;
        notaFiscal.ValorCsll = notaFiscalDto.ValorCsll;
        notaFiscal.ValorIss = notaFiscalDto.ValorIss;
        notaFiscal.ValorIssRetido = notaFiscalDto.ValorIssRetido;
        notaFiscal.DiscriminacaoServicos = notaFiscalDto.DiscriminacaoServicos;
        notaFiscal.CodigoMunicipio = notaFiscalDto.CodigoMunicipio;
        notaFiscal.Observacoes = notaFiscalDto.Observacoes;
        notaFiscal.ValorLiquido = CalcularValorLiquido(notaFiscal);
        notaFiscal.DataAtualizacao = DateTime.UtcNow;

        // Remove itens antigos (usando a coleção da nota fiscal)
        notaFiscal.ItensServico.Clear();

        // Adiciona novos itens
        foreach (var itemDto in notaFiscalDto.ItensServico)
        {
            var item = _mapper.Map<ItemServico>(itemDto);
            item.NotaFiscalId = notaFiscal.Id;
            item.ValorTotal = item.Quantidade * item.ValorUnitario;
            notaFiscal.ItensServico.Add(item);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Id} atualizada com sucesso", notaFiscal.Id);

        return _mapper.Map<NotaFiscalDTO>(notaFiscal);
    }

    public async Task<bool> ExcluirNotaFiscalAsync(int id, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();

        if (notaFiscal == null)
        {
            return false;
        }

        // Só permite excluir notas em Rascunho
        if (notaFiscal.Situacao != SituacaoNotaFiscal.Rascunho)
        {
            throw new InvalidOperationException($"Não é possível excluir uma nota fiscal com situação {notaFiscal.Situacao}. Apenas notas em Rascunho podem ser excluídas.");
        }

        // Remove a nota (os itens serão removidos em cascade)
        _context.NotasFiscais.Remove(notaFiscal);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Id} excluída com sucesso", id);

        return true;
    }

    public async Task<NotaFiscalDTO?> ObterNotaFiscalPorIdAsync(int id, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.Empresa)
            .Include(n => n.Tomador)
            .Include(n => n.ItensServico)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();

        return notaFiscal != null ? _mapper.Map<NotaFiscalDTO>(notaFiscal) : null;
    }

    public async Task<IEnumerable<NotaFiscalDTO>> ListarNotasFiscaisAsync(int empresaId)
    {
        // A empresa logada é o prestador
        // Remove Include de Empresa para evitar erro com campo ProvedorNFSe que pode não existir no banco
        var query = _context.NotasFiscais
            .Include(n => n.Tomador)
            .Where(n => n.EmpresaId == empresaId)
            .AsQueryable();

        var notas = await query.OrderByDescending(n => n.DataEmissao).ToListAsync();
        return _mapper.Map<IEnumerable<NotaFiscalDTO>>(notas);
    }

    public async Task<PagedResultDTO<NotaFiscalDTO>> ListarNotasFiscaisPaginadasAsync(int empresaId, int pageNumber, int pageSize)
    {
        // A empresa logada é o prestador
        // Remove Include de Empresa para evitar erro com campo ProvedorNFSe que pode não existir no banco
        var query = _context.NotasFiscais
            .Include(n => n.Tomador)
            .Where(n => n.EmpresaId == empresaId)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var notas = await query
            .OrderByDescending(n => n.DataEmissao)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var notasDTO = _mapper.Map<IEnumerable<NotaFiscalDTO>>(notas);

        return new PagedResultDTO<NotaFiscalDTO>
        {
            Items = notasDTO,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> CancelarNotaFiscalAsync(int id, string motivo, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.Empresa)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();
        
        if (notaFiscal == null)
        {
            return false;
        }

        if (notaFiscal.Situacao != SituacaoNotaFiscal.Autorizada)
        {
            throw new InvalidOperationException("Apenas notas autorizadas podem ser canceladas");
        }

        // Para NS Tecnologia, precisamos de chNFSe ao invés de Numero/CodigoVerificacao
        // Mas também aceitamos Numero/CodigoVerificacao como fallback
        if (string.IsNullOrWhiteSpace(notaFiscal.Numero) && string.IsNullOrWhiteSpace(notaFiscal.CodigoVerificacao))
        {
            // Tenta obter chNFSe da consulta de status se tiver nsNRec
            if (!string.IsNullOrWhiteSpace(notaFiscal.NsNRec))
            {
                try
                {
                    var empresa = await _context.Empresas
                        .Where(e => e.Id == empresaId)
                        .Select(e => new { e.CNPJ })
                        .FirstOrDefaultAsync();

                    if (empresa != null && !string.IsNullOrWhiteSpace(empresa.CNPJ))
                    {
                        var provedor = await _provedorFactory.ObterProvedorAsync(empresaId);
                        var statusResult = await provedor.ConsultarStatusAsync(notaFiscal.NsNRec, empresa.CNPJ, empresaId, notaFiscal.Id, notaFiscal.Numero);
                        
                        if (statusResult != null && !string.IsNullOrWhiteSpace(statusResult.ChNFSe))
                        {
                            // Atualiza a nota com as informações obtidas (formata o número com 6 dígitos)
                            if (string.IsNullOrWhiteSpace(notaFiscal.Numero) && statusResult.Numero != null)
                            {
                                if (int.TryParse(statusResult.Numero, out var numeroCancelamento))
                                {
                                    notaFiscal.Numero = numeroCancelamento.ToString("D6");
                                }
                                else
                                {
                                    notaFiscal.Numero = statusResult.Numero;
                                }
                            }
                            if (string.IsNullOrWhiteSpace(notaFiscal.CodigoVerificacao) && statusResult.CodigoVerificacao != null)
                            {
                                notaFiscal.CodigoVerificacao = statusResult.CodigoVerificacao;
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao tentar obter informações da nota via consulta de status antes do cancelamento");
                }
            }
            
            if (string.IsNullOrWhiteSpace(notaFiscal.Numero) || string.IsNullOrWhiteSpace(notaFiscal.CodigoVerificacao))
            {
                throw new InvalidOperationException("Nota fiscal não possui número ou código de verificação. Tente consultar o status da nota primeiro.");
            }
        }

        try
        {
            var provedor = await _provedorFactory.ObterProvedorAsync(empresaId);
            var resultado = await provedor.CancelarNotaFiscalAsync(
                notaFiscal.Numero ?? "",
                notaFiscal.CodigoVerificacao ?? "",
                motivo,
                notaFiscal.EmpresaId);
            
            if (!resultado)
            {
                throw new InvalidOperationException("Não foi possível cancelar a nota fiscal. Verifique os dados e tente novamente.");
            }

            notaFiscal.Situacao = SituacaoNotaFiscal.Cancelada;
            notaFiscal.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nota fiscal {Numero} cancelada com sucesso", notaFiscal.Numero);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal");
            throw;
        }
    }

    public async Task<NotaFiscalDTO?> ConsultarSituacaoAsync(int id, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.Empresa)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();
        
        if (notaFiscal == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(notaFiscal.Numero) || string.IsNullOrWhiteSpace(notaFiscal.CodigoVerificacao))
        {
            throw new InvalidOperationException("Nota fiscal não possui número ou código de verificação");
        }

        try
        {
            var provedor = await _provedorFactory.ObterProvedorAsync(empresaId);
            var notaConsultada = await provedor.ConsultarNotaFiscalAsync(
                notaFiscal.Numero,
                notaFiscal.CodigoVerificacao,
                notaFiscal.EmpresaId);

            if (notaConsultada != null)
            {
                notaFiscal.Situacao = (SituacaoNotaFiscal)notaConsultada.Situacao;
                notaFiscal.XML = notaConsultada.XML ?? notaFiscal.XML;
                notaFiscal.JSON = notaConsultada.JSON ?? notaFiscal.JSON;
                notaFiscal.DataAtualizacao = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<NotaFiscalDTO>(notaFiscal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar situação da nota fiscal");
            throw;
        }
    }

    /// <summary>
    /// Consulta o status da nota fiscal usando nsNRec (número do protocolo) e atualiza a situação
    /// </summary>
    public async Task<NotaFiscalDTO?> ConsultarStatusAsync(int id, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.Empresa)
            .Include(n => n.Tomador) // Inclui tomador para verificar email
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();
        
        if (notaFiscal == null)
        {
            return null;
        }

        // Precisa ter nsNRec para consultar status
        if (string.IsNullOrWhiteSpace(notaFiscal.NsNRec))
        {
            throw new InvalidOperationException("Nota fiscal não possui nsNRec (número do protocolo) para consultar status");
        }

        try
        {
            var provedor = await _provedorFactory.ObterProvedorAsync(empresaId);
            
            // Busca CNPJ da empresa
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new { e.CNPJ })
                .FirstOrDefaultAsync();

            if (empresa == null || string.IsNullOrWhiteSpace(empresa.CNPJ))
            {
                throw new InvalidOperationException("CNPJ da empresa não encontrado");
            }

            _logger.LogInformation("Consultando status da nota fiscal {Id} usando nsNRec: {NsNRec}", id, notaFiscal.NsNRec);
            
            var statusResult = await provedor.ConsultarStatusAsync(
                notaFiscal.NsNRec,
                empresa.CNPJ,
                empresaId,
                id, // Passa o ID da nota fiscal para os logs
                notaFiscal.Numero); // Passa o número da nota para os logs

            if (statusResult != null)
            {
                _logger.LogInformation("Status consultado - cStat: {CStat}", statusResult.Situacao);
                
                // Se a consulta de status retornou cStat=100 (Autorizado), atualiza a situação
                if (statusResult.Situacao == 100)
                {
                    _logger.LogInformation("Atualizando situação da nota fiscal {Id} para Autorizada", id);
                    notaFiscal.Situacao = SituacaoNotaFiscal.Autorizada;
                    
                    // Atualiza o Numero se disponível na resposta (formata com 6 dígitos)
                    if (!string.IsNullOrWhiteSpace(statusResult.Numero))
                    {
                        if (int.TryParse(statusResult.Numero, out var numero))
                        {
                            notaFiscal.Numero = numero.ToString("D6");
                            _logger.LogInformation("Número da nota atualizado e formatado: {Numero}", notaFiscal.Numero);
                        }
                        else
                        {
                            notaFiscal.Numero = statusResult.Numero;
                            _logger.LogInformation("Número da nota atualizado: {Numero}", statusResult.Numero);
                        }
                    }
                    
                    // Atualiza o CodigoVerificacao se disponível na resposta
                    if (!string.IsNullOrWhiteSpace(statusResult.CodigoVerificacao))
                    {
                        notaFiscal.CodigoVerificacao = statusResult.CodigoVerificacao;
                        _logger.LogInformation("Código de verificação atualizado: {CodigoVerificacao}", statusResult.CodigoVerificacao);
                    }
                    
                    // Atualiza o XML se disponível
                    if (!string.IsNullOrWhiteSpace(statusResult.XML))
                    {
                        notaFiscal.XML = statusResult.XML;
                    }
                    
                    // Atualiza o JSON se disponível
                    if (!string.IsNullOrWhiteSpace(statusResult.JSON))
                    {
                        notaFiscal.JSON = statusResult.JSON;
                    }
                    
                    // Tenta baixar o PDF se tiver as chaves
                    if (!string.IsNullOrWhiteSpace(statusResult.ChDPS) && !string.IsNullOrWhiteSpace(statusResult.ChNFSe))
                    {
                        try
                        {
                            byte[]? pdfBytes;
                            // Se for NS Tecnologia, usa a sobrecarga com número da nota para logs
                            if (provedor is NSTecnologiaAPIService nsTecnologia)
                            {
                                pdfBytes = await nsTecnologia.DownloadPDFAsync(statusResult.ChDPS, statusResult.ChNFSe, empresa.CNPJ, empresaId, notaFiscal.Numero, notaFiscal.Id);
                            }
                            else
                            {
                                pdfBytes = await provedor.DownloadPDFAsync(statusResult.ChDPS, statusResult.ChNFSe, empresa.CNPJ, empresaId);
                            }
                            
                            if (pdfBytes != null && pdfBytes.Length > 0)
                            {
                                var pastaBase = Path.IsPathRooted(_pastaPDF) ? _pastaPDF : Path.Combine(Directory.GetCurrentDirectory(), _pastaPDF);
                                
                                // Remove formatação do CNPJ para usar como nome da pasta
                                var cnpjEmpresa = empresa.CNPJ?.Replace(".", "").Replace("/", "").Replace("-", "").Trim() ?? "SEM_CNPJ";
                                var pastaCompleta = Path.Combine(pastaBase, cnpjEmpresa);
                                
                                if (!Directory.Exists(pastaCompleta))
                                {
                                    Directory.CreateDirectory(pastaCompleta);
                                }
                                
                                // Formata o número da nota com 6 dígitos (ex: 000006)
                                var numeroFormatado = "000000";
                                if (!string.IsNullOrWhiteSpace(notaFiscal.Numero) && int.TryParse(notaFiscal.Numero, out var numero))
                                {
                                    numeroFormatado = numero.ToString("D6");
                                }
                                
                                // Obtém o nome do tomador (precisa recarregar com Include se necessário)
                                var nomeTomador = "Sem Tomador";
                                if (notaFiscal.Tomador != null && !string.IsNullOrWhiteSpace(notaFiscal.Tomador.RazaoSocialNome))
                                {
                                    nomeTomador = notaFiscal.Tomador.RazaoSocialNome;
                                }
                                
                                // Remove caracteres inválidos do nome do arquivo
                                var caracteresInvalidos = Path.GetInvalidFileNameChars();
                                foreach (var c in caracteresInvalidos)
                                {
                                    nomeTomador = nomeTomador.Replace(c, '_');
                                }
                                
                                // Nome do arquivo: {NumeroFormatado} - {NomeTomador}.pdf
                                var nomeArquivo = $"{numeroFormatado} - {nomeTomador}.pdf";
                                
                                var caminhoCompleto = Path.Combine(pastaCompleta, nomeArquivo);
                                await File.WriteAllBytesAsync(caminhoCompleto, pdfBytes);
                                notaFiscal.PDFUrl = caminhoCompleto;
                                
                                // Formata e salva o número da nota com 6 dígitos
                                if (!string.IsNullOrWhiteSpace(notaFiscal.Numero) && int.TryParse(notaFiscal.Numero, out var numeroNotaConsulta))
                                {
                                    notaFiscal.Numero = numeroNotaConsulta.ToString("D6");
                                }
                                
                                await _context.SaveChangesAsync();
                                
                                _logger.LogInformation("PDF baixado e salvo: {Caminho}", caminhoCompleto);
                                
                                // Envia PDF por email automaticamente se a nota estiver autorizada e o tomador tiver email
                                if (notaFiscal.Situacao == SituacaoNotaFiscal.Autorizada && 
                                    notaFiscal.Tomador != null && 
                                    !string.IsNullOrWhiteSpace(notaFiscal.Tomador.Email))
                                {
                                    try
                                    {
                                        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                                        _logger.LogInformation("📧 ENVIO AUTOMÁTICO DE EMAIL (Consulta de Status)");
                                        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                                        _logger.LogInformation("Enviando PDF por email automaticamente para: {Email}", notaFiscal.Tomador.Email);
                                        
                                        var numeroNotaEmailConsulta = notaFiscal.Numero ?? notaFiscal.Id.ToString();
                                        await _emailService.EnviarPDFPorEmailAsync(
                                            notaFiscal.Tomador.Email,
                                            notaFiscal.Tomador.RazaoSocialNome,
                                            numeroNotaEmailConsulta,
                                            pdfBytes,
                                            nomeArquivo
                                        );
                                        
                                        _logger.LogInformation("✅ Email enviado com sucesso para {Email}", notaFiscal.Tomador.Email);
                                        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                                    }
                                    catch (Exception exEmail)
                                    {
                                        _logger.LogError(exEmail, "Erro ao enviar PDF por email automaticamente para {Email}. O PDF foi salvo com sucesso.", notaFiscal.Tomador.Email);
                                        // Não interrompe o fluxo, apenas loga o erro
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Erro ao baixar PDF após consulta de status (não crítico)");
                        }
                    }
                }
                else
                {
                    // Para outros status, mantém o status atual ou atualiza conforme necessário
                    _logger.LogInformation("Status consultado: {Status} - mantendo situação atual", statusResult.Situacao);
                }
                
                notaFiscal.DataAtualizacao = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Consulta de status retornou null para nota fiscal {Id}", id);
            }

            var notaDto = _mapper.Map<NotaFiscalDTO>(notaFiscal);
            // Adiciona o XMotivo do resultado da consulta ao DTO
            if (statusResult != null && !string.IsNullOrWhiteSpace(statusResult.XMotivo))
            {
                notaDto.XMotivo = statusResult.XMotivo;
            }
            return notaDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status da nota fiscal {Id}", id);
            throw;
        }
    }

    public async Task<string?> ObterXMLAsync(int id, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.Empresa)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();
        
        return notaFiscal?.XML;
    }

    private async Task ValidarDadosAsync(NotaFiscalCreateDTO notaFiscal, int empresaId)
    {
        // Valida se a empresa existe e está ativa
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null || !empresa.Ativo)
        {
            throw new InvalidOperationException("Empresa não encontrada ou inativa");
        }

        // O código do município será preenchido automaticamente da empresa se não foi informado
        // Não precisa validar aqui, será tratado no CriarNotaFiscalAsync

        // Tomador é opcional - só valida se TomadorId > 0
        if (notaFiscal.TomadorId > 0)
        {
            var tomador = await _context.Tomadores
                .FirstOrDefaultAsync(t => t.Id == notaFiscal.TomadorId && t.EmpresaId == empresaId);
            if (tomador == null)
            {
                throw new InvalidOperationException("Tomador não encontrado");
            }
        }

        // Valida se há itens de serviço
        if (notaFiscal.ItensServico == null)
        {
            _logger.LogWarning("NotaFiscalCreateDTO recebido com ItensServico = null");
            throw new InvalidOperationException("A nota fiscal deve ter pelo menos um item de serviço. A lista de itens não foi fornecida.");
        }

        if (!notaFiscal.ItensServico.Any())
        {
            _logger.LogWarning("NotaFiscalCreateDTO recebido com ItensServico vazio");
            throw new InvalidOperationException("A nota fiscal deve ter pelo menos um item de serviço. A lista de itens está vazia.");
        }

        // Valida cada item de serviço
        foreach (var item in notaFiscal.ItensServico)
        {
            if (string.IsNullOrWhiteSpace(item.CodigoServico))
            {
                throw new InvalidOperationException("Todos os itens de serviço devem ter um código de serviço.");
            }

            if (string.IsNullOrWhiteSpace(item.Discriminacao))
            {
                throw new InvalidOperationException("Todos os itens de serviço devem ter uma descrição (discriminação).");
            }

            if (item.Quantidade <= 0)
            {
                throw new InvalidOperationException("Todos os itens de serviço devem ter quantidade maior que zero.");
            }

            if (item.ValorUnitario <= 0)
            {
                throw new InvalidOperationException("Todos os itens de serviço devem ter valor unitário maior que zero.");
            }
        }

        // Valida valores
        if (notaFiscal.ValorServicos <= 0)
        {
            throw new InvalidOperationException("O valor dos serviços deve ser maior que zero");
        }
    }

    public async Task<string?> ObterMotivoRejeicaoAsync(int id, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();

        if (notaFiscal == null)
        {
            throw new InvalidOperationException("Nota fiscal não encontrada");
        }

        if (notaFiscal.Situacao != SituacaoNotaFiscal.Rejeitada)
        {
            throw new InvalidOperationException("A nota fiscal não está rejeitada");
        }

        return notaFiscal.MotivoRejeicao;
    }

    public async Task<NotaFiscalDTO> ReverterParaRascunhoAsync(int id, int empresaId)
    {
        var notaFiscal = await _context.NotasFiscais
            .Include(n => n.Tomador)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();

        if (notaFiscal == null)
        {
            throw new InvalidOperationException("Nota fiscal não encontrada");
        }

        if (notaFiscal.Situacao != SituacaoNotaFiscal.Rejeitada)
        {
            throw new InvalidOperationException($"Não é possível reverter uma nota fiscal com situação {notaFiscal.Situacao}. Apenas notas Rejeitadas podem ser revertidas para Rascunho.");
        }

        // Reverte para Rascunho e limpa dados da emissão
        notaFiscal.Situacao = SituacaoNotaFiscal.Rascunho;
        notaFiscal.Numero = null;
        notaFiscal.CodigoVerificacao = null;
        notaFiscal.XML = null;
        notaFiscal.JSON = null;
        notaFiscal.MotivoRejeicao = null; // Limpa o motivo da rejeição
        notaFiscal.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Id} revertida de Rejeitada para Rascunho", id);

        return _mapper.Map<NotaFiscalDTO>(notaFiscal);
    }

    public async Task<NotaFiscalDTO> CopiarNotaFiscalAsync(int id, int empresaId)
    {
        // Busca a nota fiscal original
        var notaFiscalOriginal = await _context.NotasFiscais
            .Include(n => n.ItensServico)
            .Include(n => n.Tomador)
            .Where(n => n.Id == id && n.EmpresaId == empresaId)
            .FirstOrDefaultAsync();

        if (notaFiscalOriginal == null)
        {
            throw new InvalidOperationException("Nota fiscal não encontrada");
        }

        // Cria um DTO com os dados da nota original para criar uma cópia
        var notaFiscalCreateDto = new NotaFiscalCreateDTO
        {
            TomadorId = notaFiscalOriginal.TomadorId,
            Serie = notaFiscalOriginal.Serie,
            Competencia = notaFiscalOriginal.Competencia,
            DataVencimento = notaFiscalOriginal.DataVencimento,
            ValorServicos = notaFiscalOriginal.ValorServicos,
            ValorDeducoes = notaFiscalOriginal.ValorDeducoes,
            ValorPis = notaFiscalOriginal.ValorPis,
            ValorCofins = notaFiscalOriginal.ValorCofins,
            ValorInss = notaFiscalOriginal.ValorInss,
            ValorIr = notaFiscalOriginal.ValorIr,
            ValorCsll = notaFiscalOriginal.ValorCsll,
            ValorIss = notaFiscalOriginal.ValorIss,
            ValorIssRetido = notaFiscalOriginal.ValorIssRetido,
            DiscriminacaoServicos = notaFiscalOriginal.DiscriminacaoServicos,
            CodigoMunicipio = notaFiscalOriginal.CodigoMunicipio,
            Observacoes = notaFiscalOriginal.Observacoes,
            ItensServico = notaFiscalOriginal.ItensServico.Select(item => new ItemServicoCreateDTO
            {
                CodigoServico = item.CodigoServico,
                Discriminacao = item.Discriminacao,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario,
                AliquotaIss = item.AliquotaIss,
                ItemListaServico = item.ItemListaServico
            }).ToList()
        };

        // Cria a nova nota fiscal (será criada com status Rascunho)
        var notaFiscalCopiada = await CriarNotaFiscalAsync(notaFiscalCreateDto, empresaId);

        _logger.LogInformation("Nota fiscal {IdOriginal} copiada para nova nota fiscal {IdNova}", id, notaFiscalCopiada.Id);

        return notaFiscalCopiada;
    }

    private decimal CalcularValorLiquido(NotaFiscal notaFiscal)
    {
        // Valor líquido = Valor dos serviços - Deduções - ISS Retido
        // Os demais impostos (PIS, COFINS, INSS, IR, CSLL) são apenas informativos
        // e não reduzem o valor líquido da nota fiscal
        return notaFiscal.ValorServicos
            - notaFiscal.ValorDeducoes
            - notaFiscal.ValorIssRetido;
    }
}

