using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NFSe2026.API.Configurations;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;

namespace NFSe2026.API.Services;

/// <summary>
/// Serviço para integração com a API da NS Tecnologia
/// </summary>
public class NSTecnologiaAPIService : IProvedorNFSeService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NSTecnologiaAPIService> _logger;
    private readonly NSTecnologiaSettings _settings;
    private readonly ICriptografiaService _criptografiaService;
    private readonly IConfiguration _configuration;
    private readonly NSTecnologiaEmissaoLogger _emissaoLogger;

    public string NomeProvedor => "NS Tecnologia";

    public NSTecnologiaAPIService(
        HttpClient httpClient,
        ApplicationDbContext context,
        ILogger<NSTecnologiaAPIService> logger,
        NSTecnologiaSettings settings,
        ICriptografiaService criptografiaService,
        IConfiguration configuration,
        NSTecnologiaEmissaoLogger emissaoLogger)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
        _settings = settings;
        _criptografiaService = criptografiaService;
        _configuration = configuration;
        _emissaoLogger = emissaoLogger;

        // Normaliza a URL base
        var urlBase = _settings.UrlBase.TrimEnd('/');
        _httpClient.BaseAddress = new Uri(urlBase);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.Timeout);

        _logger.LogInformation("NS Tecnologia API configurada - BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
    }

    public async Task<string> ObterTokenAsync(int empresaId)
    {
        try
        {
            // Token da NS Tecnologia é lido do appsettings.json (configuração global)
            // Não é necessário buscar do banco de dados por empresa
            string? token = _settings.Token;
            
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("⚠️ Token da NS Tecnologia não está configurado em 'NSTecnologia:Token' no appsettings.json");
                throw new Exception("Token da NS Tecnologia não configurado. Configure o token em 'NSTecnologia:Token' no appsettings.json");
            }

            _logger.LogInformation("Token da NS Tecnologia obtido da configuração global (appsettings.json)");
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter token da NS Tecnologia");
            throw;
        }
    }

    public async Task<EmitirNotaFiscalResult> EmitirNotaFiscalAsync(NotaFiscalCreateDTO notaFiscal, int empresaId, int? notaFiscalId = null, string? numeroNota = null)
    {
        try
        {
            _logger.LogInformation("Iniciando emissão de NFSe pela NS Tecnologia para empresa {EmpresaId}", empresaId);
            
            var token = await ObterTokenAsync(empresaId);
            _logger.LogInformation("Token obtido com sucesso para empresa {EmpresaId}", empresaId);

            // Busca dados da empresa
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new
                {
                    e.CNPJ,
                    e.InscricaoMunicipal,
                    e.CodigoMunicipio,
                    e.RazaoSocial,
                    e.NomeFantasia,
                    e.Telefone,
                    e.Email,
                    e.OptanteSimplesNacional,
                    e.RegimeEspecialTributacao,
                    e.NaturezaJuridica
                })
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                throw new Exception("Empresa não encontrada");
            }

            // Busca dados do tomador se informado
            object? tomadorData = null;
            _logger.LogInformation("🔍 Verificando tomador para emissão - TomadorId recebido: {TomadorId}", notaFiscal.TomadorId);
            
            if (notaFiscal.TomadorId > 0)
            {
                _logger.LogInformation("📋 TomadorId > 0, buscando tomador no banco de dados...");
                var tomador = await _context.Tomadores
                    .Where(t => t.Id == notaFiscal.TomadorId && t.EmpresaId == empresaId)
                    .FirstOrDefaultAsync();
                
                if (tomador != null)
                {
                    _logger.LogInformation("✅ Tomador encontrado - ID: {TomadorId}, Nome: {Nome}, CPF/CNPJ: {CPFCNPJ}", 
                        tomador.Id, tomador.RazaoSocialNome, tomador.CPFCNPJ);
                    
                    // Constrói o objeto tomador usando objetos anônimos (mesmo padrão do resto do payload)
                    // Isso garante que a serialização JSON seja consistente
                    object enderecoAnonimo;
                    if (!string.IsNullOrWhiteSpace(tomador.Complemento))
                    {
                        enderecoAnonimo = new
                        {
                            logradouro = tomador.Endereco,
                            numero = tomador.Numero,
                            complemento = tomador.Complemento,
                            bairro = tomador.Bairro,
                            cidade = tomador.Cidade,
                            uf = tomador.UF,
                            cep = tomador.CEP
                        };
                    }
                    else
                    {
                        enderecoAnonimo = new
                        {
                            logradouro = tomador.Endereco,
                            numero = tomador.Numero,
                            bairro = tomador.Bairro,
                            cidade = tomador.Cidade,
                            uf = tomador.UF,
                            cep = tomador.CEP
                        };
                    }
                    
                    var contatoAnonimo = new
                    {
                        email = tomador.Email ?? "",
                        telefone = tomador.Telefone ?? ""
                    };
                    
                    // Constrói o objeto tomador com campos obrigatórios e opcionais
                    if (!string.IsNullOrWhiteSpace(tomador.InscricaoEstadual) && 
                        !string.IsNullOrWhiteSpace(tomador.InscricaoMunicipal))
                    {
                        tomadorData = new
                        {
                            cpfCnpj = tomador.CPFCNPJ,
                            razaoSocialNome = tomador.RazaoSocialNome,
                            inscricaoEstadual = tomador.InscricaoEstadual,
                            inscricaoMunicipal = tomador.InscricaoMunicipal,
                            endereco = enderecoAnonimo,
                            contato = contatoAnonimo
                        };
                    }
                    else if (!string.IsNullOrWhiteSpace(tomador.InscricaoEstadual))
                    {
                        tomadorData = new
                        {
                            cpfCnpj = tomador.CPFCNPJ,
                            razaoSocialNome = tomador.RazaoSocialNome,
                            inscricaoEstadual = tomador.InscricaoEstadual,
                            endereco = enderecoAnonimo,
                            contato = contatoAnonimo
                        };
                    }
                    else if (!string.IsNullOrWhiteSpace(tomador.InscricaoMunicipal))
                    {
                        tomadorData = new
                        {
                            cpfCnpj = tomador.CPFCNPJ,
                            razaoSocialNome = tomador.RazaoSocialNome,
                            inscricaoMunicipal = tomador.InscricaoMunicipal,
                            endereco = enderecoAnonimo,
                            contato = contatoAnonimo
                        };
                    }
                    else
                    {
                        tomadorData = new
                        {
                            cpfCnpj = tomador.CPFCNPJ,
                            razaoSocialNome = tomador.RazaoSocialNome,
                            endereco = enderecoAnonimo,
                            contato = contatoAnonimo
                        };
                    }
                    
                    _logger.LogInformation("📦 Estrutura do tomador criada usando objetos anônimos (formato consistente com o payload)");
                }
                else
                {
                    _logger.LogWarning("⚠️ TomadorId informado ({TomadorId}) mas tomador não encontrado no banco de dados para empresa {EmpresaId}", 
                        notaFiscal.TomadorId, empresaId);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ TomadorId é {TomadorId} (zero ou negativo). Nota será emitida sem dados do tomador.", notaFiscal.TomadorId);
            }

            // Obtém o primeiro item de serviço (a API da NS Tecnologia usa um serviço por DPS)
            var primeiroItem = notaFiscal.ItensServico.FirstOrDefault();
            if (primeiroItem == null)
            {
                throw new Exception("A nota fiscal deve ter pelo menos um item de serviço");
            }

            // Determina o código do ambiente (1=Produção, 2=Homologação)
            var codigoAmbiente = _settings.Ambiente?.Equals("Producao", StringComparison.OrdinalIgnoreCase) == true ? "1" : "2";
            
            // Código do município (local de emissão)
            var codigoMunicipio = empresa.CodigoMunicipio ?? notaFiscal.CodigoMunicipio;
            if (string.IsNullOrWhiteSpace(codigoMunicipio))
            {
                throw new Exception("Código do município é obrigatório. Configure na empresa ou informe na nota fiscal.");
            }

            // Formata telefone (remove caracteres especiais, mantém apenas números)
            var telefoneFormatado = string.IsNullOrWhiteSpace(empresa.Telefone) 
                ? "0000000000" 
                : new string(empresa.Telefone.Where(char.IsDigit).ToArray()).PadLeft(10, '0').Substring(0, Math.Min(10, new string(empresa.Telefone.Where(char.IsDigit).ToArray()).Length));

            // Determina regime tributário
            // opSimpNac: 1=Simples Nacional, 2=Não optante, 3=MEI (Microempreendedor Individual)
            // MOCKADO: Sempre usa "3" (MEI) para testes
            string opSimpNac = "3"; // MEI - valor mockado
            
            _logger.LogInformation("Regime tributário determinado - opSimpNac: {OpSimpNac} (MOCKADO - sempre 3 para testes)", opSimpNac);
            
            // Regime especial de tributação (regEspTrib) - deve ser um número de 0 a 6
            // Converte "Nenhum" ou valores inválidos para "0"
            string regEspTrib = "0"; // Padrão: 0 = Nenhum regime especial
            if (!string.IsNullOrWhiteSpace(empresa.RegimeEspecialTributacao))
            {
                // Tenta converter o valor para número
                var regEspTribValue = empresa.RegimeEspecialTributacao.Trim();
                if (int.TryParse(regEspTribValue, out int regEspTribNum) && regEspTribNum >= 0 && regEspTribNum <= 6)
                {
                    regEspTrib = regEspTribNum.ToString();
                }
                else if (regEspTribValue.Equals("Nenhum", StringComparison.OrdinalIgnoreCase))
                {
                    regEspTrib = "0"; // "Nenhum" = 0
                }
                else
                {
                    _logger.LogWarning("Valor inválido para RegimeEspecialTributacao: '{Value}'. Usando padrão '0'.", regEspTribValue);
                    regEspTrib = "0";
                }
            }
            
            // Código de tributação nacional (cTribNac) - deve ter 6 dígitos (padrão [0-9]{6})
            // IMPORTANTE: Preserva zeros à esquerda do código cadastrado
            var codigoTributacaoNacional = "171102"; // Padrão
            if (!string.IsNullOrWhiteSpace(primeiroItem.CodigoServico))
            {
                var codigoOriginal = primeiroItem.CodigoServico.Trim();
                
                // Remove apenas pontos e espaços, preservando zeros à esquerda
                var codigoLimpo = codigoOriginal.Replace(".", "").Replace(" ", "").Replace("-", "").Trim();
                
                _logger.LogInformation("Processando código de tributação - Original: '{Original}', Limpo: '{Limpo}', Tamanho: {Tamanho}", 
                    codigoOriginal, codigoLimpo, codigoLimpo.Length);
                
                // Se o código limpo contém apenas dígitos e tem 6 caracteres, usa exatamente como está
                if (codigoLimpo.Length == 6 && codigoLimpo.All(char.IsDigit))
                {
                    codigoTributacaoNacional = codigoLimpo;
                    _logger.LogInformation("Código já tem 6 dígitos, usando exatamente como está: {Codigo}", codigoTributacaoNacional);
                }
                else if (codigoLimpo.All(char.IsDigit))
                {
                    // Se tem apenas dígitos mas menos de 6, preenche com zeros à esquerda
                    codigoTributacaoNacional = codigoLimpo.PadLeft(6, '0');
                    _logger.LogInformation("Código formatado com zeros à esquerda: {Codigo} (de {Original})", 
                        codigoTributacaoNacional, codigoLimpo);
                }
                else
                {
                    // Se contém caracteres não numéricos, remove todos e formata
                    var codigoApenasDigitos = new string(codigoLimpo.Where(char.IsDigit).ToArray());
                    if (!string.IsNullOrWhiteSpace(codigoApenasDigitos))
                    {
                        codigoTributacaoNacional = codigoApenasDigitos.PadLeft(6, '0');
                        if (codigoTributacaoNacional.Length > 6)
                        {
                            codigoTributacaoNacional = codigoTributacaoNacional.Substring(codigoTributacaoNacional.Length - 6);
                        }
                        _logger.LogInformation("Código formatado removendo caracteres não numéricos: {Codigo} (de {Original})", 
                            codigoTributacaoNacional, codigoOriginal);
                    }
                }
            }
            _logger.LogInformation("Código de tributação nacional final: {Codigo} (original do banco: {Original})", 
                codigoTributacaoNacional, primeiroItem.CodigoServico ?? "null");
            
            // Código NBS (cNBS) - deve ter 9 dígitos sem ponto (padrão [0-9]{9})
            // Remove pontos, espaços e formata para 9 dígitos preenchendo com zeros à esquerda
            var codigoNBS = "103012200"; // Padrão
            if (!string.IsNullOrWhiteSpace(primeiroItem.ItemListaServico))
            {
                // Remove pontos, espaços e qualquer caractere não numérico
                var nbsLimpo = new string(primeiroItem.ItemListaServico.Where(char.IsDigit).ToArray());
                if (!string.IsNullOrWhiteSpace(nbsLimpo))
                {
                    // Preenche com zeros à esquerda até ter 9 dígitos
                    codigoNBS = nbsLimpo.PadLeft(9, '0');
                    // Se tiver mais de 9 dígitos, pega os últimos 9
                    if (codigoNBS.Length > 9)
                    {
                        codigoNBS = codigoNBS.Substring(codigoNBS.Length - 9);
                    }
                }
            }
            _logger.LogInformation("Código NBS formatado: {Codigo} (original: {Original})", 
                codigoNBS, primeiroItem.ItemListaServico ?? "null");
            
            // Descrição do serviço
            var descricaoServico = primeiroItem.Discriminacao ?? notaFiscal.DiscriminacaoServicos;
            
            // Total de tributos do Simples Nacional (PIS + COFINS + INSS + IR + CSLL)
            var totalTributosSN = notaFiscal.ValorPis + notaFiscal.ValorCofins + notaFiscal.ValorInss + notaFiscal.ValorIr + notaFiscal.ValorCsll;
            
            // Monta o payload conforme a estrutura da API da NS Tecnologia
            // Estrutura baseada no exemplo fornecido pela documentação
            // Constrói infDPS dinamicamente para incluir tomador condicionalmente
            var infDPSObj = new Dictionary<string, object>
            {
                { "tpAmb", codigoAmbiente },
                { "dhEmi", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") },
                { "verAplic", "NS1.0.0" },
                { "serie", notaFiscal.Serie ?? "1" },
                { "nDPS", notaFiscalId?.ToString() ?? "1" },
                { "dCompet", notaFiscal.Competencia.ToString("yyyy-MM-dd") },
                { "tpEmit", "1" },
                { "cLocEmi", codigoMunicipio },
                { "prest", new
                    {
                        CNPJ = empresa.CNPJ,
                        fone = telefoneFormatado,
                        email = empresa.Email ?? "Exemplo@email.com",
                        regTrib = new
                        {
                            opSimpNac = opSimpNac,
                            regApTribSN = "1",
                            regEspTrib = regEspTrib
                        }
                    }
                },
                { "serv", new
                    {
                        locPrest = new
                        {
                            cLocPrestacao = codigoMunicipio
                        },
                        cServ = new
                        {
                            cTribNac = codigoTributacaoNacional,
                            xDescServ = descricaoServico,
                            cNBS = codigoNBS
                        }
                    }
                },
                { "valores", new
                    {
                        vServPrest = new
                        {
                            vServ = notaFiscal.ValorServicos.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        },
                        trib = new
                        {
                            tribMun = new
                            {
                                tribISSQN = "1",
                                tpRetISSQN = "1"
                            },
                            tribFed = new
                            {
                                piscofins = new
                                {
                                    CST = "08"
                                }
                            },
                            totTrib = new
                            {
                                pTotTribSN = totalTributosSN.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                            }
                        }
                    }
                }
            };

            // Adiciona tomador ao payload se existir
            if (tomadorData != null)
            {
                infDPSObj["tomador"] = tomadorData;
                _logger.LogInformation("Tomador incluído no payload: {TomadorId}", notaFiscal.TomadorId);
            }
            else
            {
                _logger.LogWarning("⚠️ Nota fiscal não possui tomador (TomadorId: {TomadorId}). A emissão será feita sem dados do tomador.", notaFiscal.TomadorId);
            }

            var payload = new
            {
                DPS = new
                {
                    versao = "1.01",
                    infDPS = infDPSObj
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
            { 
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            
            _logger.LogInformation("Payload JSON gerado para NS Tecnologia. Tamanho: {Tamanho} caracteres", json.Length);
            _logger.LogDebug("Payload JSON: {Payload}", json);

            // Content-Type deve ser definido no HttpContent, não no HttpRequestMessage
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Endpoint de emissão: /nfse/issue (usado tanto em homologação quanto em produção)
            var endpoint = _settings.EndpointEmitir ?? "/nfse/issue";
            
            _logger.LogInformation("Usando endpoint de emissão: {Endpoint} (Ambiente: {Ambiente})", endpoint, _settings.Ambiente);
            _logger.LogInformation("Enviando requisição para endpoint: {Endpoint}", endpoint);
            
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            // Adiciona token no header conforme documentação da NS Tecnologia
            // A NS Tecnologia usa o header X-AUTH-TOKEN para autenticação
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Add("X-AUTH-TOKEN", token);
                _logger.LogInformation("Token de autenticação adicionado no header X-AUTH-TOKEN");
            }
            else
            {
                _logger.LogWarning("⚠️ Token vazio - a requisição pode falhar por falta de autenticação");
            }
            
            // Accept header pode ser adicionado no HttpRequestMessage
            request.Headers.Add("Accept", "application/json");
            // Content-Type já está definido no StringContent acima, não precisa adicionar aqui

            _logger.LogInformation("📤 PASSO 1.1 - Enviando requisição HTTP POST para {BaseAddress}{Endpoint}", _httpClient.BaseAddress, endpoint);
            
            // Log específico de emissão - PASSO 1.1
            // Usa o número da nota se disponível, caso contrário usa o ID
            var identificadorLog = NSTecnologiaEmissaoLogger.ObterIdentificadorLog(null, numeroNota, notaFiscalId ?? 0);
            var logDadosPaso1_1 = new Dictionary<string, object>
            {
                { "CNPJ", empresa.CNPJ },
                { "ValorServicos", notaFiscal.ValorServicos },
                { "Endpoint", endpoint },
                { "Payload", json }
            };
            var logConteudo1_1 = _emissaoLogger.FormatLogCompleto(notaFiscalId ?? 0, "PASSO 1.1 - ENVIO", "Enviando requisição de emissão", logDadosPaso1_1);
            _emissaoLogger.LogEmissao(identificadorLog, logConteudo1_1);
            
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 PASSO 1.2 - Resposta recebida da NS Tecnologia. Status: {StatusCode}, Tamanho: {Tamanho} caracteres", 
                response.StatusCode, responseContent.Length);
            _logger.LogInformation("=== RESPOSTA COMPLETA DA NS TECNOLOGIA (PASSO 1 - EMISSÃO) ===\n{Resposta}\n===================================================", responseContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("NFSe emitida com sucesso na NS Tecnologia");
                
                // Tenta deserializar resposta conforme estrutura da NS Tecnologia
                try
                {
                    var resultado = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Extrai status e xmotivo se status for 200
                    string? xmotivo = null;
                    int statusResponse = 0;
                    if (resultado.TryGetProperty("status", out var statusPropResponse))
                    {
                        statusResponse = statusPropResponse.ValueKind == JsonValueKind.Number 
                            ? statusPropResponse.GetInt32() 
                            : (int.TryParse(statusPropResponse.GetString(), out var statusInt) ? statusInt : 0);
                        
                        if (statusResponse == 200)
                        {
                            if (resultado.TryGetProperty("xMotivo", out var xMotivoProp))
                            {
                                xmotivo = xMotivoProp.ValueKind == JsonValueKind.String 
                                    ? xMotivoProp.GetString() 
                                    : xMotivoProp.ToString();
                            }
                            else if (resultado.TryGetProperty("xmotivo", out var xmotivoProp))
                            {
                                xmotivo = xmotivoProp.ValueKind == JsonValueKind.String 
                                    ? xmotivoProp.GetString() 
                                    : xmotivoProp.ToString();
                            }
                        }
                    }
                    
                    // Log específico de emissão - PASSO 1.2 (Resposta)
                    var logDadosPaso1_2 = new Dictionary<string, object>
                    {
                        { "StatusHTTP", response.StatusCode.ToString() },
                        { "StatusAPI", statusResponse },
                        { "XMotivo", xmotivo ?? "Não informado" },
                        { "RespostaCompleta", responseContent }
                    };
                    var logConteudo1_2 = _emissaoLogger.FormatLogCompleto(notaFiscalId ?? 0, "PASSO 1.2 - RESPOSTA", "Resposta da emissão", logDadosPaso1_2);
                    _emissaoLogger.LogEmissao(identificadorLog, logConteudo1_2);
                    
                    // Tenta extrair URL do PDF da resposta
                    string? pdfUrl = null;
                    if (resultado.TryGetProperty("pdfUrl", out var pdfUrlProp))
                    {
                        pdfUrl = pdfUrlProp.GetString();
                    }
                    else if (resultado.TryGetProperty("pdf", out var pdfProp))
                    {
                        pdfUrl = pdfProp.GetString();
                    }
                    else if (resultado.TryGetProperty("danfseUrl", out var danfseUrlProp))
                    {
                        pdfUrl = danfseUrlProp.GetString();
                    }
                    else if (resultado.TryGetProperty("urlPdf", out var urlPdfProp))
                    {
                        pdfUrl = urlPdfProp.GetString();
                    }
                    
                    _logger.LogInformation("PDF URL extraído da resposta: {PDFUrl}", pdfUrl ?? "Não encontrado");
                    
                    // Extrai nsNRec (número do protocolo de recebimento) da resposta
                    // Pode vir como número ou string
                    string? nsNRec = null;
                    if (resultado.TryGetProperty("nsNRec", out var nsNRecProp))
                    {
                        nsNRec = nsNRecProp.ValueKind == JsonValueKind.String 
                            ? nsNRecProp.GetString() 
                            : nsNRecProp.ToString();
                    }
                    else if (resultado.TryGetProperty("numeroRecibo", out var numeroReciboProp))
                    {
                        nsNRec = numeroReciboProp.ValueKind == JsonValueKind.String 
                            ? numeroReciboProp.GetString() 
                            : numeroReciboProp.ToString();
                    }
                    else if (resultado.TryGetProperty("nRec", out var nRecProp))
                    {
                        nsNRec = nRecProp.ValueKind == JsonValueKind.String 
                            ? nRecProp.GetString() 
                            : nRecProp.ToString();
                    }
                    
                    _logger.LogInformation("nsNRec extraído da resposta: {NsNRec}", nsNRec ?? "Não encontrado");
                    
                    // Tenta extrair número da nota fiscal de vários campos possíveis
                    // Pode vir como número ou string
                    string? numero = null;
                    if (resultado.TryGetProperty("numero", out var numeroProp))
                    {
                        numero = numeroProp.ValueKind == JsonValueKind.String 
                            ? numeroProp.GetString() 
                            : numeroProp.ToString();
                        _logger.LogInformation("Número encontrado no campo 'numero': {Numero}", numero);
                    }
                    else if (resultado.TryGetProperty("numeroNFSe", out var numNFSeProp))
                    {
                        numero = numNFSeProp.ValueKind == JsonValueKind.String 
                            ? numNFSeProp.GetString() 
                            : numNFSeProp.ToString();
                        _logger.LogInformation("Número encontrado no campo 'numeroNFSe': {Numero}", numero);
                    }
                    else if (resultado.TryGetProperty("nNFSe", out var nNFSeProp))
                    {
                        numero = nNFSeProp.ValueKind == JsonValueKind.String 
                            ? nNFSeProp.GetString() 
                            : nNFSeProp.ToString();
                        _logger.LogInformation("Número encontrado no campo 'nNFSe': {Numero}", numero);
                    }
                    else if (resultado.TryGetProperty("nDPS", out var nDPSProp))
                    {
                        numero = nDPSProp.ValueKind == JsonValueKind.String 
                            ? nDPSProp.GetString() 
                            : nDPSProp.ToString();
                        _logger.LogInformation("Número encontrado no campo 'nDPS': {Numero}", numero);
                    }
                    else if (resultado.TryGetProperty("nfse", out var nfseProp) && nfseProp.ValueKind == JsonValueKind.Object)
                    {
                        // Se houver um objeto "nfse", tenta extrair o número dele
                        if (nfseProp.TryGetProperty("numero", out var nfseNumeroProp))
                        {
                            numero = nfseNumeroProp.ValueKind == JsonValueKind.String 
                                ? nfseNumeroProp.GetString() 
                                : nfseNumeroProp.ToString();
                            _logger.LogInformation("Número encontrado no campo 'nfse.numero': {Numero}", numero);
                        }
                    }
                    
                    if (string.IsNullOrWhiteSpace(numero))
                    {
                        _logger.LogWarning("Número da nota fiscal não encontrado na resposta da NS Tecnologia. Campos disponíveis: {Campos}", 
                            string.Join(", ", resultado.EnumerateObject().Select(p => p.Name)));
                    }
                    
                    // Tenta extrair código de verificação
                    // Pode vir como número ou string
                    string? codigoVerificacao = null;
                    if (resultado.TryGetProperty("codigoVerificacao", out var codigoProp))
                    {
                        codigoVerificacao = codigoProp.ValueKind == JsonValueKind.String 
                            ? codigoProp.GetString() 
                            : codigoProp.ToString();
                        _logger.LogInformation("Código de verificação encontrado no campo 'codigoVerificacao': {Codigo}", codigoVerificacao);
                    }
                    else if (resultado.TryGetProperty("codigoVerificacaoNFSe", out var codVerifProp))
                    {
                        codigoVerificacao = codVerifProp.ValueKind == JsonValueKind.String 
                            ? codVerifProp.GetString() 
                            : codVerifProp.ToString();
                        _logger.LogInformation("Código de verificação encontrado no campo 'codigoVerificacaoNFSe': {Codigo}", codigoVerificacao);
                    }
                    else if (resultado.TryGetProperty("codVerificacao", out var codVerifProp2))
                    {
                        codigoVerificacao = codVerifProp2.ValueKind == JsonValueKind.String 
                            ? codVerifProp2.GetString() 
                            : codVerifProp2.ToString();
                        _logger.LogInformation("Código de verificação encontrado no campo 'codVerificacao': {Codigo}", codigoVerificacao);
                    }
                    
                    var resultadoFinal = new EmitirNotaFiscalResult
                    {
                        Numero = numero,
                        CodigoVerificacao = codigoVerificacao,
                        XML = resultado.TryGetProperty("xml", out var xmlProp) ? xmlProp.GetString() : null,
                        JSON = responseContent,
                        PDFUrl = pdfUrl,
                        NsNRec = nsNRec,
                        // Quando status é 200, significa "enviado para Sefaz" - usa status "Enviada" (5)
                        Situacao = statusResponse == 200 ? (int)SituacaoNotaFiscal.Enviada :
                                 resultado.TryGetProperty("situacao", out var situacaoProp) ? situacaoProp.GetInt32() : 
                                 (int)SituacaoNotaFiscal.Rascunho
                    };
                    
                    // Atualiza o identificador do log para usar o número da nota quando disponível (caso não tenha sido passado inicialmente)
                    if (!string.IsNullOrWhiteSpace(numero) && string.IsNullOrWhiteSpace(numeroNota))
                    {
                        identificadorLog = NSTecnologiaEmissaoLogger.ObterIdentificadorLog(null, numero, notaFiscalId ?? 0);
                    }
                    
                    // Log específico de emissão - PASSO 1.3 (Resultado Final)
                    var logDadosPaso1_3 = new Dictionary<string, object>
                    {
                        { "Numero", numero ?? "null" },
                        { "CodigoVerificacao", codigoVerificacao ?? "null" },
                        { "NsNRec", nsNRec ?? "null" },
                        { "Situacao", resultadoFinal.Situacao },
                        { "PDFUrl", pdfUrl ?? "null" },
                        { "XMotivo", xmotivo ?? "Não informado" }
                    };
                    var logConteudo1_3 = _emissaoLogger.FormatLogCompleto(notaFiscalId ?? 0, "PASSO 1.3 - RESULTADO FINAL", "Resultado da emissão", logDadosPaso1_3);
                    _emissaoLogger.LogEmissao(identificadorLog, logConteudo1_3);
                    
                    return resultadoFinal;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Erro ao deserializar resposta da NS Tecnologia. Retornando resposta bruta.");
                    return new EmitirNotaFiscalResult
                    {
                        JSON = responseContent,
                        Situacao = (int)SituacaoNotaFiscal.Rascunho // Mantém como Rascunho quando não é possível determinar o status (erro na resposta)
                    };
                }
            }
            else
            {
                _logger.LogError("Erro ao emitir NFSe na NS Tecnologia. Status: {StatusCode}, Resposta: {Response}", 
                    response.StatusCode, responseContent);
                
                // Tenta extrair mensagem de erro da resposta
                string mensagemErro = $"Erro ao emitir NFSe na NS Tecnologia: {response.StatusCode}";
                try
                {
                    var erroJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (erroJson.TryGetProperty("message", out var msgProp))
                        mensagemErro += $" - {msgProp.GetString()}";
                    else if (erroJson.TryGetProperty("error", out var errProp))
                        mensagemErro += $" - {errProp.GetString()}";
                    else
                        mensagemErro += $" - {responseContent}";
                }
                catch
                {
                    mensagemErro += $" - {responseContent}";
                }
                
                throw new Exception(mensagemErro);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal na NS Tecnologia para empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<bool> CancelarNotaFiscalAsync(string numero, string codigoVerificacao, string motivo, int empresaId)
    {
        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
        _logger.LogInformation("🚫 INICIANDO CANCELAMENTO DE NFSe");
        _logger.LogInformation("═══════════════════════════════════════════════════════════════");
        _logger.LogInformation("Número: {Numero}, Código Verificação: {CodigoVerificacao}, Motivo: {Motivo}", 
            numero, codigoVerificacao, motivo);
        
        try
        {
            var token = await ObterTokenAsync(empresaId);
            
            // Busca CNPJ da empresa
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new { e.CNPJ })
                .FirstOrDefaultAsync();
            
            if (empresa == null || string.IsNullOrWhiteSpace(empresa.CNPJ))
            {
                _logger.LogWarning("CNPJ não encontrado para empresa {EmpresaId}. Continuando cancelamento sem CNPJ no payload.", empresaId);
            }

            var payload = new Dictionary<string, object>
            {
                { "X-AUTH-TOKEN", token ?? "" },
                { "numero", numero },
                { "codigoVerificacao", codigoVerificacao },
                { "motivo", motivo }
            };
            
            // Adiciona CNPJ apenas se foi obtido
            if (empresa != null && !string.IsNullOrWhiteSpace(empresa.CNPJ))
            {
                payload["CNPJ"] = empresa.CNPJ;
            }

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
            { 
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            
            // Log específico de cancelamento - PASSO 1.1
            // Para cancelamento, usa o número da nota como identificador
            var identificadorCancelamento = NSTecnologiaEmissaoLogger.ObterIdentificadorLog(null, numero, 0);
            var logDadosPaso1_1 = new Dictionary<string, object>
            {
                { "Numero", numero },
                { "CodigoVerificacao", codigoVerificacao },
                { "Motivo", motivo },
                { "CNPJ", empresa?.CNPJ ?? "Não informado" },
                { "Endpoint", _settings.EndpointCancelar ?? "/nfse/cancelar" },
                { "Payload", json }
            };
            var logConteudo1_1 = _emissaoLogger.FormatLogCompleto(0, "CANCELAMENTO 1.1 - ENVIO", "Enviando requisição de cancelamento", logDadosPaso1_1);
            _emissaoLogger.LogEmissao(identificadorCancelamento, logConteudo1_1);
            
            // Content-Type é definido automaticamente pelo StringContent
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var endpoint = _settings.EndpointCancelar ?? "/nfse/cancelar";
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            // X-AUTH-TOKEN agora está no body, não precisa no header
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("📥 Resposta recebida da NS Tecnologia. Status: {StatusCode}, Tamanho: {Tamanho} caracteres", 
                response.StatusCode, responseContent.Length);

            // Tenta deserializar resposta para verificar status
            try
            {
                var resultado = JsonSerializer.Deserialize<JsonElement>(responseContent);
                int statusValue = 0;
                string? motivoErro = null;
                string? xmotivo = null;
                
                if (resultado.TryGetProperty("status", out var statusProp))
                {
                    statusValue = statusProp.ValueKind == JsonValueKind.Number 
                        ? statusProp.GetInt32() 
                        : (int.TryParse(statusProp.GetString(), out var statusInt) ? statusInt : 0);
                }
                
                // Tenta extrair motivo/xMotivo
                if (resultado.TryGetProperty("motivo", out var motivoProp))
                {
                    motivoErro = motivoProp.GetString();
                }
                else if (resultado.TryGetProperty("xMotivo", out var xMotivoProp))
                {
                    xmotivo = xMotivoProp.ValueKind == JsonValueKind.String 
                        ? xMotivoProp.GetString() 
                        : xMotivoProp.ToString();
                }
                else if (resultado.TryGetProperty("xmotivo", out var xmotivoProp))
                {
                    xmotivo = xmotivoProp.ValueKind == JsonValueKind.String 
                        ? xmotivoProp.GetString() 
                        : xmotivoProp.ToString();
                }
                
                // Log específico de cancelamento - PASSO 1.2
                var logDadosPaso1_2 = new Dictionary<string, object>
                {
                    { "StatusHTTP", (int)response.StatusCode },
                    { "StatusAPI", statusValue },
                    { "Motivo", motivoErro ?? xmotivo ?? "Não informado" },
                    { "XMotivo", xmotivo ?? "Não informado" },
                    { "RespostaCompleta", responseContent }
                };
                var logConteudo1_2 = _emissaoLogger.FormatLogCompleto(0, "CANCELAMENTO 1.2 - RESPOSTA", "Resposta do cancelamento", logDadosPaso1_2);
                _emissaoLogger.LogEmissao(identificadorCancelamento, logConteudo1_2);
                
                if (response.IsSuccessStatusCode && statusValue == 200)
                {
                    _logger.LogInformation("✅ NFSe {Numero} cancelada com sucesso na NS Tecnologia", numero);
                    _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                    return true;
                }
                else
                {
                    var mensagemErroFinal = motivoErro ?? xmotivo ?? "Erro desconhecido";
                    _logger.LogError("❌ Erro ao cancelar NFSe na NS Tecnologia. Status HTTP: {StatusCode}, Status API: {StatusAPI}, Motivo: {Motivo}",
                        response.StatusCode, statusValue, mensagemErroFinal);
                    _logger.LogInformation("═══════════════════════════════════════════════════════════════");
                    throw new InvalidOperationException($"Erro ao cancelar nota fiscal: {mensagemErroFinal}");
                }
            }
            catch (JsonException ex)
            {
                // Se não conseguir deserializar, loga e trata como erro
                _logger.LogError(ex, "Erro ao deserializar resposta do cancelamento");
                var logDadosErro = new Dictionary<string, object>
                {
                    { "StatusHTTP", (int)response.StatusCode },
                    { "Erro", "Falha ao deserializar resposta JSON" },
                    { "RespostaCompleta", responseContent }
                };
                var logConteudoErro = _emissaoLogger.FormatLogCompleto(0, "CANCELAMENTO 1.2 - ERRO", "Erro ao processar resposta", logDadosErro);
                _emissaoLogger.LogEmissao(identificadorCancelamento, logConteudoErro);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ NFSe {Numero} cancelada com sucesso (não foi possível validar resposta JSON)", numero);
                    return true;
                }
                else
                {
                    throw new InvalidOperationException($"Erro ao cancelar nota fiscal: Status HTTP {response.StatusCode}");
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Re-throw InvalidOperationException sem alterar (já tem a mensagem correta)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro inesperado ao cancelar nota fiscal na NS Tecnologia");
            _logger.LogInformation("═══════════════════════════════════════════════════════════════");
            
            // Log de erro
            var identificadorErro = NSTecnologiaEmissaoLogger.ObterIdentificadorLog(null, numero, 0);
            var logDadosErro = new Dictionary<string, object>
            {
                { "Erro", ex.Message },
                { "StackTrace", ex.StackTrace ?? "Não disponível" }
            };
            var logConteudoErro = _emissaoLogger.FormatLogCompleto(0, "CANCELAMENTO - ERRO INESPERADO", "Erro inesperado no cancelamento", logDadosErro);
            _emissaoLogger.LogEmissao(identificadorErro, logConteudoErro);
            
            throw new InvalidOperationException($"Erro ao cancelar nota fiscal: {ex.Message}", ex);
        }
    }

    public async Task<ConsultarNotaFiscalResult?> ConsultarNotaFiscalAsync(string numero, string codigoVerificacao, int empresaId)
    {
        try
        {
            var token = await ObterTokenAsync(empresaId);

            var endpoint = _settings.EndpointConsultar ?? "/nfse/consultar";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}?numero={numero}&codigoVerificacao={codigoVerificacao}");
            
            // Usa X-AUTH-TOKEN conforme documentação da NS Tecnologia
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Add("X-AUTH-TOKEN", token);
            }
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("=== RESPOSTA COMPLETA DA NS TECNOLOGIA (CONSULTA) ===\n{Resposta}\n===================================================", responseContent);

            if (response.IsSuccessStatusCode)
            {
                // TODO: Deserializar resposta conforme estrutura da NS Tecnologia
                var resultado = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                return new ConsultarNotaFiscalResult
                {
                    Numero = numero,
                    CodigoVerificacao = codigoVerificacao,
                    Situacao = resultado.TryGetProperty("situacao", out var situacaoProp) ? situacaoProp.GetInt32() : 0,
                    XML = resultado.TryGetProperty("xml", out var xmlProp) ? xmlProp.GetString() : null,
                    ChDPS = resultado.TryGetProperty("chDPS", out var chDPSProp) ? chDPSProp.GetString() : null,
                    ChNFSe = resultado.TryGetProperty("chNFSe", out var chNFSeProp) ? chNFSeProp.GetString() : null
                };
            }
            else
            {
                _logger.LogWarning("Erro ao consultar NFSe na NS Tecnologia. Status: {StatusCode}, Resposta: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar nota fiscal na NS Tecnologia");
            throw;
        }
    }
    
    /// <summary>
    /// Consulta o status de processamento da NFSe usando nsNRec (número do protocolo)
    /// </summary>
    public async Task<ConsultarNotaFiscalResult?> ConsultarStatusAsync(string nsNRec, string cnpj, int empresaId, int? notaFiscalId = null, string? numeroNota = null)
    {
        try
        {
            var token = await ObterTokenAsync(empresaId);

            var payload = new Dictionary<string, object>
            {
                { "X-AUTH-TOKEN", token ?? "" },
                { "CNPJ", cnpj },
                { "nsNRec", nsNRec }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var endpoint = _settings.EndpointStatus ?? "/nfse/issue/status";
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            request.Headers.Add("Accept", "application/json");

            _logger.LogInformation("📤 PASSO 2.1 - Consultando status de processamento - nsNRec: {nsNRec}, CNPJ: {CNPJ}", nsNRec, cnpj);

            // Se numeroNota não foi fornecido mas temos notaFiscalId, busca do banco
            if (string.IsNullOrWhiteSpace(numeroNota) && notaFiscalId.HasValue && notaFiscalId.Value > 0)
            {
                try
                {
                    var nota = await _context.NotasFiscais
                        .Where(n => n.Id == notaFiscalId.Value)
                        .Select(n => n.Numero)
                        .FirstOrDefaultAsync();
                    
                    if (!string.IsNullOrWhiteSpace(nota))
                    {
                        numeroNota = nota;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao buscar número da nota fiscal {NotaFiscalId} do banco de dados", notaFiscalId.Value);
                }
            }

            // Log específico de emissão - PASSO 2.1
            // Usa o número da nota se disponível, caso contrário usa o ID
            var identificadorConsulta = NSTecnologiaEmissaoLogger.ObterIdentificadorLog(null, numeroNota, notaFiscalId ?? 0);
            var logDadosPaso2_1 = new Dictionary<string, object>
            {
                { "CNPJ", cnpj },
                { "nsNRec", nsNRec },
                { "Endpoint", endpoint },
                { "Payload", json }
            };
            var logConteudo2_1 = _emissaoLogger.FormatLogCompleto(notaFiscalId ?? 0, "PASSO 2.1 - CONSULTA STATUS", "Enviando consulta de status", logDadosPaso2_1);
            _emissaoLogger.LogEmissao(identificadorConsulta, logConteudo2_1);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 PASSO 2.2 - Resposta recebida da NS Tecnologia. Status: {StatusCode}, Tamanho: {Tamanho} caracteres", 
                response.StatusCode, responseContent.Length);
            _logger.LogInformation("=== RESPOSTA COMPLETA DA NS TECNOLOGIA (PASSO 2 - CONSULTAR STATUS) ===\n{Resposta}\n===================================================", responseContent);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var resultado = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    var status = resultado.TryGetProperty("status", out var statusProp) ? statusProp.GetInt32() : 0;
                    
                    // Extrai xMotivo se status for 200 (tenta "xMotivo" primeiro, depois "xmotivo")
                    string? xmotivo = null;
                    if (status == 200)
                    {
                        if (resultado.TryGetProperty("xMotivo", out var xMotivoProp))
                        {
                            xmotivo = xMotivoProp.ValueKind == JsonValueKind.String 
                                ? xMotivoProp.GetString() 
                                : xMotivoProp.ToString();
                        }
                        else if (resultado.TryGetProperty("xmotivo", out var xmotivoProp))
                        {
                            xmotivo = xmotivoProp.ValueKind == JsonValueKind.String 
                                ? xmotivoProp.GetString() 
                                : xmotivoProp.ToString();
                        }
                    }
                    
                    if (status == 200)
                    {
                        // Extrai as chaves chDPS e chNFSe da resposta
                        // Trata quando a API retorna a string literal "null" como null real
                        string? chDPS = null;
                        if (resultado.TryGetProperty("chDPS", out var chDPSProp))
                        {
                            if (chDPSProp.ValueKind == JsonValueKind.String)
                            {
                                var chDPSValue = chDPSProp.GetString();
                                chDPS = string.IsNullOrWhiteSpace(chDPSValue) || chDPSValue.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : chDPSValue;
                            }
                            else if (chDPSProp.ValueKind == JsonValueKind.Null)
                            {
                                chDPS = null;
                            }
                            else
                            {
                                chDPS = chDPSProp.ToString();
                            }
                        }
                        
                        string? chNFSe = null;
                        if (resultado.TryGetProperty("chNFSe", out var chNFSeProp))
                        {
                            if (chNFSeProp.ValueKind == JsonValueKind.String)
                            {
                                var chNFSeValue = chNFSeProp.GetString();
                                chNFSe = string.IsNullOrWhiteSpace(chNFSeValue) || chNFSeValue.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : chNFSeValue;
                            }
                            else if (chNFSeProp.ValueKind == JsonValueKind.Null)
                            {
                                chNFSe = null;
                            }
                            else
                            {
                                chNFSe = chNFSeProp.ToString();
                            }
                        }
                        
                        _logger.LogInformation("Status consultado com sucesso. chDPS: {chDPS}, chNFSe: {chNFSe}", chDPS ?? "null", chNFSe ?? "null");
                        
                        // Extrai número da nota e código de verificação do XML ou JSON
                        string? numeroNFSe = null;
                        string? codigoVerificacao = null;
                        
                        // Tenta extrair do XML primeiro
                        if (resultado.TryGetProperty("xml", out var xmlProp) && xmlProp.ValueKind == JsonValueKind.String)
                        {
                            var xmlContent = xmlProp.GetString();
                            if (!string.IsNullOrWhiteSpace(xmlContent))
                            {
                            try
                            {
                                var xmlDoc = new System.Xml.XmlDocument();
                                xmlDoc.LoadXml(xmlContent);
                                var nfseNamespace = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
                                nfseNamespace.AddNamespace("nfse", "http://www.sped.fazenda.gov.br/nfse");
                                
                                // Extrai nNFSe (número da nota)
                                var nNFSeNode = xmlDoc.SelectSingleNode("//nfse:infNFSe/nfse:nNFSe", nfseNamespace) 
                                    ?? xmlDoc.SelectSingleNode("//nNFSe");
                                if (nNFSeNode != null && !string.IsNullOrWhiteSpace(nNFSeNode.InnerText))
                                {
                                    numeroNFSe = nNFSeNode.InnerText;
                                    _logger.LogInformation("Número da nota extraído do XML: {Numero}", numeroNFSe);
                                }
                                
                                // Tenta extrair código de verificação (pode estar em diferentes locais)
                                // Geralmente não vem no XML de consulta de status, mas verificamos
                            }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Erro ao extrair número do XML da resposta");
                                }
                            }
                        }
                        
                        // Se não encontrou no XML, tenta no JSON direto
                        if (string.IsNullOrWhiteSpace(numeroNFSe))
                        {
                            if (resultado.TryGetProperty("numero", out var numeroProp))
                            {
                                numeroNFSe = numeroProp.ValueKind == JsonValueKind.String 
                                    ? numeroProp.GetString() 
                                    : numeroProp.ToString();
                            }
                            else if (resultado.TryGetProperty("nNFSe", out var nNFSeProp))
                            {
                                numeroNFSe = nNFSeProp.ValueKind == JsonValueKind.String 
                                    ? nNFSeProp.GetString() 
                                    : nNFSeProp.ToString();
                            }
                        }
                        
                        // Log específico de emissão - PASSO 2.2 (Resultado)
                        var logDadosPaso2_2 = new Dictionary<string, object>
                        {
                            { "StatusAPI", status },
                            { "XMotivo", xmotivo ?? "Não informado" },
                            { "chDPS", chDPS ?? "null" },
                            { "chNFSe", chNFSe ?? "null" },
                            { "Numero", numeroNFSe ?? "null" },
                            { "RespostaCompleta", responseContent }
                        };
                        // Identificador sempre usa o ID da nota fiscal (não muda mais)
                        var logConteudo2_2 = _emissaoLogger.FormatLogCompleto(notaFiscalId ?? 0, "PASSO 2.2 - RESULTADO CONSULTA STATUS", "Resultado da consulta de status", logDadosPaso2_2);
                        _emissaoLogger.LogEmissao(identificadorConsulta, logConteudo2_2);
                        
                        // Extrai cStat (pode vir como string ou número)
                        int situacaoFinal = status;
                        string? cStatValue = null;
                        if (resultado.TryGetProperty("cStat", out var cStatProp))
                        {
                            if (cStatProp.ValueKind == JsonValueKind.Number)
                            {
                                situacaoFinal = cStatProp.GetInt32();
                                cStatValue = situacaoFinal.ToString();
                            }
                            else if (cStatProp.ValueKind == JsonValueKind.String)
                            {
                                cStatValue = cStatProp.GetString();
                                if (!string.IsNullOrWhiteSpace(cStatValue) && int.TryParse(cStatValue, out var cStatInt))
                                {
                                    situacaoFinal = cStatInt;
                                }
                                else
                                {
                                    // Se cStat não pode ser parseado como número (ex: "E999"), mantém o status original (200)
                                    // mas registra um warning
                                    _logger.LogWarning("cStat inválido ou não numérico recebido: {cStat}. Mantendo status {Status}. xMotivo: {XMotivo}", 
                                        cStatValue, status, xmotivo ?? "Não informado");
                                }
                            }
                        }
                        
                        // Se houver xMotivo indicando erro, registra um warning mesmo que status seja 200
                        if (!string.IsNullOrWhiteSpace(xmotivo) && 
                            (xmotivo.Contains("Falha", StringComparison.OrdinalIgnoreCase) || 
                             xmotivo.Contains("erro", StringComparison.OrdinalIgnoreCase) ||
                             xmotivo.Contains("Erro", StringComparison.OrdinalIgnoreCase) ||
                             (!string.IsNullOrWhiteSpace(cStatValue) && cStatValue.StartsWith("E", StringComparison.OrdinalIgnoreCase))))
                        {
                            _logger.LogWarning("⚠️ A consulta de status retornou status 200, mas há indicadores de erro: xMotivo='{XMotivo}', cStat='{cStat}'. A nota pode ainda estar em processamento.", 
                                xmotivo, cStatValue ?? "não informado");
                        }
                        
                        var result = new ConsultarNotaFiscalResult
                        {
                            Numero = numeroNFSe,
                            CodigoVerificacao = codigoVerificacao,
                            ChDPS = chDPS,
                            ChNFSe = chNFSe,
                            NsNRec = nsNRec,
                            Situacao = situacaoFinal,
                            XML = resultado.TryGetProperty("xml", out var xmlPropResult) ? xmlPropResult.GetString() : null,
                            JSON = responseContent,
                            XMotivo = xmotivo
                        };
                        
                        _logger.LogInformation("🔍 ConsultarStatusAsync retornando - ChDPS: {ChDPS}, ChNFSe: {ChNFSe}, NsNRec: {NsNRec}", 
                            result.ChDPS ?? "null", result.ChNFSe ?? "null", result.NsNRec ?? "null");
                        
                        return result;
                    }
                    else
                    {
                        var motivo = resultado.TryGetProperty("motivo", out var motivoProp) ? motivoProp.GetString() : "Erro desconhecido";
                        _logger.LogWarning("Erro ao consultar status. Status: {Status}, Motivo: {Motivo}", status, motivo);
                        return null;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Erro ao deserializar resposta da consulta de status");
                    return null;
                }
            }
            else
            {
                _logger.LogError("Erro ao consultar status. Status HTTP: {StatusCode}, Resposta: {Response}", 
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status de processamento na NS Tecnologia");
            throw;
        }
    }

    public async Task<byte[]?> DownloadPDFAsync(string chDPS, string chNFSe, string cnpj, int empresaId)
    {
        return await DownloadPDFAsync(chDPS, chNFSe, cnpj, empresaId, null, 0);
    }

    /// <summary>
    /// Faz download do PDF da nota fiscal (sobrecarga com número da nota para logs)
    /// </summary>
    public async Task<byte[]?> DownloadPDFAsync(string chDPS, string chNFSe, string cnpj, int empresaId, string? numeroNota, int notaFiscalId)
    {
        try
        {
            var token = await ObterTokenAsync(empresaId);
            
            // Determina o código do ambiente (1=Produção, 2=Homologação)
            var codigoAmbiente = _settings.Ambiente?.Equals("Producao", StringComparison.OrdinalIgnoreCase) == true ? "1" : "2";

            var payload = new Dictionary<string, object>
            {
                { "X-AUTH-TOKEN", token ?? "" },
                { "chDPS", chDPS },
                { "chNFSe", chNFSe },
                { "CNPJ", cnpj },
                { "tpDown", "xp" }, // XML e PDF
                { "tpAmb", codigoAmbiente }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var endpoint = "/nfse/get";
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            request.Headers.Add("Accept", "application/json");

            _logger.LogInformation("📤 PASSO 3.1 - Fazendo download do PDF da NFSe - chDPS: {chDPS}, chNFSe: {chNFSe}", chDPS, chNFSe);

            // Log específico de emissão - PASSO 3.1
            // Usa o mesmo identificador do log principal (número da nota ou ID)
            var identificadorLog = NSTecnologiaEmissaoLogger.ObterIdentificadorLog(null, numeroNota, notaFiscalId);
            var logDadosPaso3_1 = new Dictionary<string, object>
            {
                { "CNPJ", cnpj },
                { "chDPS", chDPS },
                { "chNFSe", chNFSe },
                { "Endpoint", endpoint },
                { "Payload", json }
            };
            var logConteudo3_1 = _emissaoLogger.FormatLogCompleto(notaFiscalId, "PASSO 3.1 - DOWNLOAD PDF", "Enviando requisição de download PDF", logDadosPaso3_1);
            _emissaoLogger.LogEmissao(identificadorLog, logConteudo3_1);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 PASSO 3.2 - Resposta recebida da NS Tecnologia. Status: {StatusCode}, Tamanho: {Tamanho} caracteres", 
                response.StatusCode, responseContent.Length);
            _logger.LogInformation("=== RESPOSTA COMPLETA DA NS TECNOLOGIA (PASSO 3 - DOWNLOAD PDF) ===\nTamanho da resposta: {Tamanho} caracteres\n{RespostaPreview}\n===================================================", 
                responseContent.Length, responseContent.Length > 1000 ? responseContent.Substring(0, 1000) + "..." : responseContent);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var resultado = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    var status = resultado.TryGetProperty("status", out var statusProp) ? statusProp.GetInt32() : 0;
                    
                    // Extrai xMotivo se status for 200 (tenta "xMotivo" primeiro, depois "xmotivo")
                    string? xmotivo = null;
                    if (status == 200)
                    {
                        if (resultado.TryGetProperty("xMotivo", out var xMotivoProp))
                        {
                            xmotivo = xMotivoProp.ValueKind == JsonValueKind.String 
                                ? xMotivoProp.GetString() 
                                : xMotivoProp.ToString();
                        }
                        else if (resultado.TryGetProperty("xmotivo", out var xmotivoProp))
                        {
                            xmotivo = xmotivoProp.ValueKind == JsonValueKind.String 
                                ? xmotivoProp.GetString() 
                                : xmotivoProp.ToString();
                        }
                    }
                    
                    if (status == 200)
                    {
                        // PDF está em Base64 no campo pdfDocumento
                        if (resultado.TryGetProperty("pdfDocumento", out var pdfProp))
                        {
                            var pdfBase64 = pdfProp.GetString();
                            if (!string.IsNullOrWhiteSpace(pdfBase64))
                            {
                                var pdfBytes = Convert.FromBase64String(pdfBase64);
                                _logger.LogInformation("PDF baixado com sucesso. Tamanho: {Tamanho} bytes", pdfBytes.Length);
                                
                                // Log específico de emissão - PASSO 3.2 (Resultado)
                                var logDadosPaso3_2 = new Dictionary<string, object>
                                {
                                    { "StatusAPI", status },
                                    { "XMotivo", xmotivo ?? "Não informado" },
                                    { "PDFTamanho", pdfBytes.Length },
                                    { "RespostaPreview", responseContent.Length > 1000 ? responseContent.Substring(0, 1000) + "..." : responseContent }
                                };
                                var logConteudo3_2 = _emissaoLogger.FormatLogCompleto(notaFiscalId, "PASSO 3.2 - RESULTADO DOWNLOAD PDF", "Resultado do download PDF", logDadosPaso3_2);
                                _emissaoLogger.LogEmissao(identificadorLog, logConteudo3_2);
                                
                                return pdfBytes;
                            }
                        }
                    }
                    else
                    {
                        var motivo = resultado.TryGetProperty("motivo", out var motivoProp) ? motivoProp.GetString() : "Erro desconhecido";
                        _logger.LogError("Erro ao baixar PDF. Status: {Status}, Motivo: {Motivo}", status, motivo);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Erro ao deserializar resposta do download de PDF");
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Erro ao decodificar Base64 do PDF");
                }
            }
            else
            {
                _logger.LogError("Erro ao fazer download do PDF. Status: {StatusCode}, Resposta: {Response}", 
                    response.StatusCode, responseContent);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer download do PDF da NS Tecnologia");
            throw;
        }
    }
}

