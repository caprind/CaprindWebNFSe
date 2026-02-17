using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NFSe2026.API.Configurations;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;
using NFSe2026.API.Services;

namespace NFSe2026.API.Services;

public class NFSeAPIService : INFSeAPIService
{
    public string NomeProvedor => "API Nacional NFSe";
    
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NFSeAPIService> _logger;
    private readonly ApiNacionalNFSeSettings _settings;
    private readonly IAssinaturaXMLService _assinaturaXML;
    private readonly IGeradorXMLDPSService _geradorXMLDPS;
    private readonly ICriptografiaService _criptografiaService;
    private readonly IGeradorJWTService _geradorJWT;
    private readonly IConfiguration _configuration;
    private readonly string _pastaXML;
    private readonly bool _salvarXMLAntesEnvio;
    private string? _cachedToken;
    private DateTime? _tokenExpiry;

    public NFSeAPIService(
        HttpClient httpClient,
        ApplicationDbContext context,
        ILogger<NFSeAPIService> logger,
        ApiNacionalNFSeSettings settings,
        IAssinaturaXMLService assinaturaXML,
        IGeradorXMLDPSService geradorXMLDPS,
        ICriptografiaService criptografiaService,
        IGeradorJWTService geradorJWT,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
        _settings = settings;
        _assinaturaXML = assinaturaXML;
        _geradorXMLDPS = geradorXMLDPS;
        _criptografiaService = criptografiaService;
        _geradorJWT = geradorJWT;
        _configuration = configuration;
        
        // Normaliza a URL base (remove barra final se existir)
        var urlBase = _settings.UrlBase.TrimEnd('/');
        _httpClient.BaseAddress = new Uri(urlBase);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.Timeout);
        
        _logger.LogInformation("API Nacional NFSe configurada - BaseAddress: {BaseAddress}, EndpointToken: {EndpointToken}, EndpointDPS: {EndpointDPS}", 
            _httpClient.BaseAddress, _settings.EndpointToken ?? "/oauth/token", _settings.EndpointDPS ?? "/api/v1/dps");
        
        // Configuração da pasta de documentos XML
        _pastaXML = _configuration["Documentos:PastaXML"] ?? "Documentos/XMLs";
        _salvarXMLAntesEnvio = _configuration.GetValue<bool>("Documentos:SalvarXMLAntesEnvio", true);
        
        _logger.LogInformation("Configuração de salvamento de XML - Pasta: {Pasta}, Habilitado: {Habilitado}", _pastaXML, _salvarXMLAntesEnvio);
        
        // Cria a pasta se não existir
        if (_salvarXMLAntesEnvio)
        {
            try
            {
                var pastaCompleta = Path.IsPathRooted(_pastaXML) ? _pastaXML : Path.Combine(Directory.GetCurrentDirectory(), _pastaXML);
                _logger.LogInformation("Caminho completo da pasta XML: {PastaCompleta}", pastaCompleta);
                
                if (!Directory.Exists(pastaCompleta))
                {
                    Directory.CreateDirectory(pastaCompleta);
                    _logger.LogInformation("Pasta de documentos XML criada: {Pasta}", pastaCompleta);
                }
                else
                {
                    _logger.LogInformation("Pasta de documentos XML já existe: {Pasta}", pastaCompleta);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar a pasta de documentos XML: {Pasta}", _pastaXML);
            }
        }
        else
        {
            _logger.LogWarning("Salvamento de XML está DESABILITADO na configuração");
        }
    }

    public async Task<string> ObterTokenAsync(int empresaId)
    {
        // Verifica se há token válido em cache (validade curta de 5 minutos)
        if (_cachedToken != null && _tokenExpiry.HasValue && DateTime.UtcNow < _tokenExpiry.Value)
        {
            _logger.LogInformation("Usando token de autenticação em cache");
            return _cachedToken;
        }

        try
        {
            // Usa Select para garantir que os campos do certificado e credenciais sejam carregados
            var empresa = await _context.Empresas
                .Where(e => e.Id == empresaId)
                .Select(e => new
                {
                    e.Id,
                    e.CNPJ,
                    e.Ambiente,
                    e.CertificadoDigital,
                    e.SenhaCertificado,
                    e.ClientId,
                    e.ClientSecret
                })
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                _logger.LogError("Empresa {EmpresaId} não encontrada no banco de dados", empresaId);
                throw new Exception("Empresa não encontrada");
            }

            // Valida se o certificado digital está cadastrado com logs detalhados
            _logger.LogInformation("Verificando certificado digital para empresa {EmpresaId} - CNPJ: {CNPJ}", empresaId, empresa.CNPJ);
            _logger.LogInformation("CertificadoDigital está null: {IsNull}, vazio: {IsEmpty}", 
                empresa.CertificadoDigital == null, 
                string.IsNullOrEmpty(empresa.CertificadoDigital));
            _logger.LogInformation("SenhaCertificado está null: {IsNull}, vazio: {IsEmpty}", 
                empresa.SenhaCertificado == null, 
                string.IsNullOrEmpty(empresa.SenhaCertificado));
            
            if (string.IsNullOrWhiteSpace(empresa.CertificadoDigital))
            {
                _logger.LogError("CertificadoDigital não encontrado para empresa {EmpresaId}. Campo está null ou vazio no banco de dados.", empresaId);
                throw new Exception("Certificado digital não cadastrado para a empresa. Por favor, cadastre um certificado A1 ou A3 no perfil da empresa antes de emitir notas fiscais.");
            }

            if (string.IsNullOrWhiteSpace(empresa.SenhaCertificado))
            {
                _logger.LogError("SenhaCertificado não encontrada para empresa {EmpresaId}. Campo está null ou vazio no banco de dados.", empresaId);
                throw new Exception("Senha do certificado digital não cadastrada para a empresa. Por favor, cadastre a senha do certificado no perfil da empresa.");
            }

            // Carrega o certificado digital
            _logger.LogInformation("Descriptografando senha do certificado para empresa {EmpresaId}", empresaId);
            string senhaDescriptografada;
            try
            {
                senhaDescriptografada = _criptografiaService.Descriptografar(empresa.SenhaCertificado);
                _logger.LogInformation("Senha descriptografada com sucesso. Tamanho da senha: {Tamanho} caracteres", senhaDescriptografada.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descriptografar senha do certificado. A senha pode estar corrompida ou em formato inválido.");
                throw new Exception("Erro ao descriptografar a senha do certificado digital. Tente cadastrar o certificado novamente.", ex);
            }
            
            _logger.LogInformation("Carregando certificado digital (tamanho Base64: {Tamanho} caracteres)", 
                empresa.CertificadoDigital?.Length ?? 0);
            
            X509Certificate2 certificado;
            try
            {
                certificado = _assinaturaXML.CarregarCertificado(empresa.CertificadoDigital!, senhaDescriptografada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar certificado digital para empresa {EmpresaId}. Verifique se o certificado e a senha estão corretos.", empresaId);
                throw; // Re-throw para manter a mensagem de erro específica do método CarregarCertificado
            }

            // Gera JWT assinado com certificado digital (RS256)
            _logger.LogInformation("Gerando JWT assinado com certificado digital para CNPJ {CNPJ} no ambiente {Ambiente}", 
                empresa.CNPJ, empresa.Ambiente);
            var jwtAssinado = _geradorJWT.GerarJWTAssinado(certificado, empresa.CNPJ, empresa.Ambiente.ToString());

            // Obtém ClientId e ClientSecret da empresa (criptografados) ou das settings como fallback
            string? clientId = null;
            string? clientSecret = null;
            
            if (!string.IsNullOrWhiteSpace(empresa.ClientId) && !string.IsNullOrWhiteSpace(empresa.ClientSecret))
            {
                // Descriptografa as credenciais da empresa
                try
                {
                    clientId = _criptografiaService.Descriptografar(empresa.ClientId);
                    clientSecret = _criptografiaService.Descriptografar(empresa.ClientSecret);
                    _logger.LogInformation("ClientId e ClientSecret obtidos do banco de dados da empresa (descriptografados)");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao descriptografar ClientId/ClientSecret da empresa. Tentando usar configuração global como fallback.");
                }
            }
            
            // Fallback para configuração global se não houver na empresa
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                clientId = _settings.ClientId;
                clientSecret = _settings.ClientSecret;
                if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
                {
                    _logger.LogInformation("Usando ClientId e ClientSecret da configuração global (appsettings.json)");
                }
                else
                {
                    _logger.LogWarning("⚠️ ClientId ou ClientSecret não estão configurados nem na empresa nem na configuração global. A API pode exigir essas credenciais para autenticação.");
                    _logger.LogWarning("Configure as credenciais no perfil da empresa ou em 'ApiNacionalNFSe:ClientId' e 'ApiNacionalNFSe:ClientSecret' no appsettings.json");
                }
            }

            // Envia JWT para endpoint de token
            // Monta o body da requisição
            var requestBody = new Dictionary<string, object>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                { "assertion", jwtAssinado }
            };

            // Adiciona client_id e client_secret no body se estiverem configurados
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                requestBody["client_id"] = clientId;
            }
            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                requestBody["client_secret"] = clientSecret;
            }
            if (!string.IsNullOrWhiteSpace(_settings.Scope))
            {
                requestBody["scope"] = _settings.Scope;
            }

            var json = JsonSerializer.Serialize(requestBody);
            
            // Determina a URL base para autenticação (pode ser diferente da URL base principal)
            var urlBaseAuth = !string.IsNullOrWhiteSpace(_settings.UrlBaseAuth) 
                ? _settings.UrlBaseAuth.TrimEnd('/')
                : _settings.UrlBase.TrimEnd('/');
            
            if (string.IsNullOrWhiteSpace(urlBaseAuth))
            {
                _logger.LogError("❌ URL base de autenticação está vazia! Verifique a configuração 'ApiNacionalNFSe:UrlBaseAuth' ou 'ApiNacionalNFSe:UrlBase' no appsettings.json");
                throw new Exception("URL base de autenticação não configurada. Configure 'ApiNacionalNFSe:UrlBaseAuth' no appsettings.json");
            }
            
            _logger.LogInformation("Usando URL base de autenticação: {UrlBaseAuth}", urlBaseAuth);
            _logger.LogInformation("Configuração - UrlBase: {UrlBase}, UrlBaseAuth: {UrlBaseAuth}, EndpointToken: {EndpointToken}", 
                _settings.UrlBase, _settings.UrlBaseAuth ?? "não configurado", _settings.EndpointToken ?? "não configurado");
            
            // Lista de endpoints alternativos para tentar (em ordem de prioridade)
            var endpointsParaTentar = new List<string>();
            
            // Adiciona o endpoint configurado primeiro
            if (!string.IsNullOrEmpty(_settings.EndpointToken))
            {
                endpointsParaTentar.Add(_settings.EndpointToken);
            }
            
            // Adiciona endpoints alternativos comuns
            endpointsParaTentar.AddRange(new[]
            {
                "/oauth/token",
                "/api/oauth/token",
                "/api/v1/oauth/token",
                "/api/auth/token",
                "/auth/token",
                "/token"
            });
            
            // Remove duplicatas mantendo a ordem
            endpointsParaTentar = endpointsParaTentar.Distinct().ToList();
            
            Exception? ultimoErro = null;
            string? ultimaUrlTentada = null;
            
            // Tenta cada endpoint até encontrar um que funcione
            foreach (var endpointToken in endpointsParaTentar)
            {
                try
                {
                    // Remove barra inicial se existir
                    var endpointNormalizado = endpointToken.StartsWith("/") 
                        ? endpointToken.Substring(1) 
                        : endpointToken;
                    
                    // Monta a URL completa usando a URL base de autenticação
                    var urlCompleta = $"{urlBaseAuth}/{endpointNormalizado}";
                    _logger.LogInformation("Tentando obter token no endpoint: {UrlCompleta}", urlCompleta);
                    
                    // Cria uma requisição isolada para cada tentativa com a URL completa
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, urlCompleta)
                    {
                        Content = content
                    };
                    
                    // Adiciona Basic Authentication no header se client_id e client_secret estiverem configurados
                    if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
                    {
                        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                        request.Headers.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                        _logger.LogInformation("Adicionando Basic Authentication com ClientId e ClientSecret");
                    }
                    
                    // Usa um HttpClient temporário para fazer a requisição com a URL completa
                    using var httpClientTemp = new HttpClient();
                    httpClientTemp.Timeout = TimeSpan.FromSeconds(_settings.Timeout);
                    var response = await httpClientTemp.SendAsync(request);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("✅ Token obtido com sucesso do endpoint: {UrlCompleta}", urlCompleta);
                        // Continua com o processamento do token abaixo
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                        if (tokenResponse.TryGetProperty("access_token", out var tokenElement))
                        {
                            _cachedToken = tokenElement.GetString();
                            
                            // O token retornado pela API tem validade curta (normalmente 5 minutos)
                            int expiresIn = 300; // 5 minutos em segundos (padrão)
                            if (tokenResponse.TryGetProperty("expires_in", out var expiresInElement))
                            {
                                if (expiresInElement.ValueKind == JsonValueKind.Number)
                                {
                                    expiresIn = expiresInElement.GetInt32();
                                }
                            }
                            
                            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
                            _logger.LogInformation("Access token obtido com sucesso. Expira em {ExpiresIn} segundos", expiresIn);
                            
                            // Salva o endpoint que funcionou para uso futuro
                            if (_settings.EndpointToken != endpointToken)
                            {
                                _logger.LogWarning("Endpoint configurado ({Configurado}) não funcionou. Endpoint que funcionou: {Funcionou}. Considere atualizar a configuração.", 
                                    _settings.EndpointToken ?? "não configurado", endpointToken);
                            }
                            
                            return _cachedToken ?? throw new Exception("Token não encontrado na resposta");
                        }

                        throw new Exception("Token não encontrado na resposta da API");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ultimoErro = new Exception($"Status {response.StatusCode}: {errorContent}");
                        ultimaUrlTentada = urlCompleta;
                        
                        _logger.LogWarning("Endpoint {UrlCompleta} retornou {StatusCode}. Tentando próximo endpoint...", 
                            urlCompleta, response.StatusCode);
                    }
                }
                catch (Exception ex) when (ex != ultimoErro)
                {
                    ultimoErro = ex;
                    // Garante que ultimaUrlTentada seja definida mesmo em caso de exceção
                    if (string.IsNullOrEmpty(ultimaUrlTentada))
                    {
                        try
                        {
                            var endpointNormalizado = endpointToken.StartsWith("/") 
                                ? endpointToken.Substring(1) 
                                : endpointToken;
                            ultimaUrlTentada = $"{urlBaseAuth}/{endpointNormalizado}";
                        }
                        catch
                        {
                            ultimaUrlTentada = $"URL base: {urlBaseAuth ?? "vazia"}, Endpoint: {endpointToken}";
                        }
                    }
                    _logger.LogWarning(ex, "Erro ao tentar endpoint {EndpointToken} (URL: {UrlTentada}). Tipo de erro: {TipoErro}. Tentando próximo...", 
                        endpointToken, ultimaUrlTentada, ex.GetType().Name);
                }
            }
            
            // Se chegou aqui, nenhum endpoint funcionou
            var errorContentFinal = ultimoErro?.Message ?? "Erro desconhecido";
            var tipoErro = ultimoErro?.GetType().Name ?? "Desconhecido";
            
            // Mensagem mais detalhada baseada no tipo de erro
            string mensagemErroFinal;
            if (errorContentFinal.Contains("não é conhecido") || errorContentFinal.Contains("host") || tipoErro.Contains("HttpRequestException"))
            {
                mensagemErroFinal = $"❌ Erro de conectividade ao acessar {urlBaseAuth}. " +
                    $"O host pode não estar acessível ou há problema de DNS/rede. " +
                    $"Verifique sua conexão com a internet e se o domínio {urlBaseAuth} está acessível. " +
                    $"Última URL tentada: {ultimaUrlTentada ?? "não disponível"}. " +
                    $"Erro: {errorContentFinal}";
            }
            else
            {
                mensagemErroFinal = $"❌ Nenhum endpoint de autenticação funcionou. Tentados: {string.Join(", ", endpointsParaTentar)}. " +
                    $"Última URL tentada: {ultimaUrlTentada ?? "não disponível"}. " +
                    $"URL base de autenticação: {urlBaseAuth}. " +
                    $"Verifique o manual da API ADN para o endpoint correto e configure em 'ApiNacionalNFSe:EndpointToken' no appsettings.json. " +
                    $"Erro ({tipoErro}): {errorContentFinal}";
            }
            
            _logger.LogError(mensagemErroFinal);
            throw new Exception(mensagemErroFinal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter token de autenticação");
            throw;
        }
    }

    public async Task<EmitirNotaFiscalResult> EmitirNotaFiscalAsync(NotaFiscalCreateDTO notaFiscal, int empresaId, int? notaFiscalId = null, string? numeroNota = null)
    {
        string? xmlAssinado = null;
        string? xmlDPS = null;
        try
        {
            // Busca a empresa para obter o certificado digital
            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
            {
                throw new Exception("Empresa não encontrada");
            }

            // Valida se o certificado digital está cadastrado
            if (string.IsNullOrEmpty(empresa.CertificadoDigital) || string.IsNullOrEmpty(empresa.SenhaCertificado))
            {
                throw new Exception("Certificado digital não cadastrado para a empresa. É necessário cadastrar um certificado A1 para assinar o XML.");
            }

            // Gera o XML do DPS ANTES de tentar obter o token (para garantir que seja salvo mesmo se houver erro)
            try
            {
                _logger.LogInformation("Gerando XML DPS para nota fiscal da empresa {EmpresaId}", empresaId);
                xmlDPS = await _geradorXMLDPS.GerarXMLDPSAsync(notaFiscal, empresaId);
                _logger.LogInformation("XML DPS gerado com sucesso. Tamanho: {Tamanho} caracteres", xmlDPS?.Length ?? 0);
                
                // Salva o XML não assinado se habilitado
                if (_salvarXMLAntesEnvio && !string.IsNullOrEmpty(xmlDPS))
                {
                    try
                    {
                        await SalvarXMLParaAnaliseAsync(xmlDPS, empresaId, notaFiscal, sufixo: "_NAO_ASSINADO");
                        _logger.LogInformation("✅ XML não assinado salvo com sucesso");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Não foi possível salvar XML não assinado. Continuando...");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO ao gerar XML DPS. Tentando salvar XML parcial se disponível...");
                // Se houver algum XML parcial, tenta salvar
                if (!string.IsNullOrEmpty(xmlDPS) && _salvarXMLAntesEnvio)
                {
                    try
                    {
                        await SalvarXMLParaAnaliseAsync(xmlDPS, empresaId, notaFiscal, sufixo: "_ERRO_GERACAO");
                        _logger.LogInformation("✅ XML parcial salvo após erro na geração");
                    }
                    catch { }
                }
                throw; // Re-lança o erro
            }

            // Carrega e descriptografa a senha do certificado
            var senhaDescriptografada = _criptografiaService.Descriptografar(empresa.SenhaCertificado);
            
            // Carrega o certificado A1
            var certificado = _assinaturaXML.CarregarCertificado(empresa.CertificadoDigital, senhaDescriptografada);
            
            // Assina o XML com o certificado A1
            try
            {
                _logger.LogInformation("Assinando XML DPS com certificado digital");
                xmlAssinado = _assinaturaXML.AssinarXML(xmlDPS!, certificado);
                
                _logger.LogInformation("XML assinado gerado. Tamanho: {Tamanho} caracteres", xmlAssinado?.Length ?? 0);

                // Salva o XML assinado em arquivo ANTES de tentar obter o token ou enviar (para análise)
                if (_salvarXMLAntesEnvio && !string.IsNullOrEmpty(xmlAssinado))
                {
                    _logger.LogInformation("Tentando salvar XML assinado para análise (EmpresaId: {EmpresaId}, Tamanho XML: {Tamanho} caracteres)", 
                        empresaId, xmlAssinado.Length);
                    try
                    {
                        await SalvarXMLParaAnaliseAsync(xmlAssinado, empresaId, notaFiscal);
                        _logger.LogInformation("✅ XML assinado salvo com sucesso para análise");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ ERRO ao salvar XML assinado para análise. Continuando com o envio...");
                        // Não bloqueia o envio se falhar ao salvar
                    }
                }
                else
                {
                    _logger.LogWarning("Salvamento de XML está DESABILITADO - XML não será salvo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO ao assinar XML. Tentando salvar XML não assinado...");
                // Se falhar na assinatura, salva o XML não assinado
                if (!string.IsNullOrEmpty(xmlDPS) && _salvarXMLAntesEnvio)
                {
                    try
                    {
                        await SalvarXMLParaAnaliseAsync(xmlDPS, empresaId, notaFiscal, sufixo: "_ERRO_ASSINATURA");
                        _logger.LogInformation("✅ XML não assinado salvo após erro na assinatura");
                    }
                    catch { }
                }
                throw; // Re-lança o erro
            }

            // Agora tenta obter o token e enviar
            var token = await ObterTokenAsync(empresaId);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Envia o XML assinado
            var content = new StringContent(xmlAssinado, Encoding.UTF8, "application/xml");

            // Endpoint da API ADN para envio de XML
            var endpointDPS = _settings.EndpointDPS ?? "/api/v1/dps";
            // Remove barra inicial se existir (já está no BaseAddress)
            if (endpointDPS.StartsWith("/"))
            {
                endpointDPS = endpointDPS.Substring(1);
            }
            
            _logger.LogInformation("Enviando XML DPS assinado para a API Nacional - Endpoint: {EndpointDPS}", endpointDPS);
            var response = await _httpClient.PostAsync(endpointDPS, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao emitir NFS-e: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                throw new Exception($"Erro ao emitir NFS-e: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            // A resposta pode ser XML ou JSON dependendo da API
            // Tenta parsear como JSON primeiro, se falhar, trata como XML
            try
            {
                var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return ConverterRespostaParaResult(apiResponse);
            }
            catch
            {
                // Se não for JSON, trata como XML
                return ConverterRespostaXMLParaResult(responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal");
            
            // Tenta salvar qualquer XML que tenha sido gerado (assinado ou não)
            if (_salvarXMLAntesEnvio)
            {
                try
                {
                    if (!string.IsNullOrEmpty(xmlAssinado))
                    {
                        _logger.LogInformation("Tentando salvar XML assinado após erro no envio (EmpresaId: {EmpresaId})", empresaId);
                        await SalvarXMLParaAnaliseAsync(xmlAssinado, empresaId, notaFiscal, sufixo: "_ERRO_ENVIO");
                        _logger.LogInformation("✅ XML assinado salvo após erro no envio");
                    }
                    else if (xmlDPS != null && !string.IsNullOrEmpty(xmlDPS))
                    {
                        _logger.LogInformation("Tentando salvar XML não assinado após erro (EmpresaId: {EmpresaId})", empresaId);
                        await SalvarXMLParaAnaliseAsync(xmlDPS, empresaId, notaFiscal, sufixo: "_ERRO_ENVIO_NAO_ASSINADO");
                        _logger.LogInformation("✅ XML não assinado salvo após erro no envio");
                    }
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "❌ Erro ao salvar XML após falha no envio");
                }
            }
            
            throw;
        }
    }

    public async Task<bool> CancelarNotaFiscalAsync(string numero, string codigoVerificacao, string motivo, int empresaId)
    {
        try
        {
            var token = await ObterTokenAsync(empresaId);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                numero = numero,
                codigo_verificacao = codigoVerificacao,
                motivo = motivo
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Endpoint de cancelamento da API ADN
            var endpointDPS = _settings.EndpointDPS ?? "/api/v1/dps";
            if (endpointDPS.StartsWith("/"))
            {
                endpointDPS = endpointDPS.Substring(1);
            }
            var endpointCancelar = $"{endpointDPS}/{numero}/cancelar";
            _logger.LogInformation("Cancelando DPS {Numero} - Endpoint: {Endpoint}", numero, endpointCancelar);
            var response = await _httpClient.PostAsync(endpointCancelar, content);
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal");
            throw;
        }
    }

    public async Task<ConsultarNotaFiscalResult?> ConsultarNotaFiscalAsync(string numero, string codigoVerificacao, int empresaId)
    {
        try
        {
            var token = await ObterTokenAsync(empresaId);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Endpoint de consulta da API ADN
            var endpointDPS = _settings.EndpointDPS ?? "/api/v1/dps";
            if (endpointDPS.StartsWith("/"))
            {
                endpointDPS = endpointDPS.Substring(1);
            }
            var endpointConsulta = $"{endpointDPS}/{numero}?codigo_verificacao={codigoVerificacao}";
            _logger.LogInformation("Consultando DPS {Numero} - Endpoint: {Endpoint}", numero, endpointConsulta);
            var response = await _httpClient.GetAsync(endpointConsulta);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return ConverterRespostaParaConsultarResult(apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar nota fiscal");
            throw;
        }
    }

    private async Task<object> ConverterParaPayloadAPI(NotaFiscalCreateDTO notaFiscal, int empresaId)
    {
        // Busca dados da empresa (prestador)
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null)
        {
            throw new Exception("Empresa não encontrada");
        }

        // Busca dados do tomador
        var tomador = await _context.Tomadores
            .FirstOrDefaultAsync(t => t.Id == notaFiscal.TomadorId && t.EmpresaId == empresaId);
        if (tomador == null)
        {
            throw new Exception("Tomador não encontrado");
        }

        // Pega o primeiro item de serviço (a API ADN pode aceitar apenas um serviço por DPS)
        var primeiroItem = notaFiscal.ItensServico.FirstOrDefault();
        if (primeiroItem == null)
        {
            throw new Exception("A nota fiscal deve ter pelo menos um item de serviço");
        }

        // Gera ID da DPS (formato: DPS-{ID da Nota Fiscal} ou sequencial)
        // Por enquanto usa um ID baseado em timestamp, mas pode ser ajustado para usar o ID da nota fiscal
        var dpsId = $"DPS-{DateTime.UtcNow:yyyyMMddHHmmss}-{empresaId}-{notaFiscal.TomadorId}";

        // Determina código do município do tomador (usa o do prestador se não disponível)
        var codigoMunicipioTomador = GetCodigoMunicipioByCidadeUF(tomador.Cidade, tomador.UF);
        if (string.IsNullOrWhiteSpace(codigoMunicipioTomador))
        {
            // Se não encontrou, usa o código do município do prestador
            codigoMunicipioTomador = empresa.CodigoMunicipio ?? notaFiscal.CodigoMunicipio;
        }

        // Constrói o payload conforme estrutura ADN
        var payload = new
        {
            id = dpsId,
            competencia = notaFiscal.Competencia.ToString("yyyy-MM"),
            prestador = new
            {
                cpfCnpj = empresa.CNPJ,
                inscricaoMunicipal = empresa.InscricaoMunicipal ?? string.Empty,
                codigoMunicipio = empresa.CodigoMunicipio ?? notaFiscal.CodigoMunicipio
            },
            tomador = new
            {
                cpfCnpj = tomador.CPFCNPJ,
                razaoSocial = tomador.RazaoSocialNome,
                endereco = new
                {
                    logradouro = tomador.Endereco,
                    numero = tomador.Numero,
                    bairro = tomador.Bairro,
                    codigoMunicipio = codigoMunicipioTomador,
                    uf = tomador.UF,
                    cep = tomador.CEP
                }
            },
            servico = new
            {
                codigo = primeiroItem.CodigoServico, // ou ItemListaServico, conforme documentação
                descricao = string.IsNullOrWhiteSpace(primeiroItem.Discriminacao) 
                    ? notaFiscal.DiscriminacaoServicos 
                    : primeiroItem.Discriminacao,
                aliquota = primeiroItem.AliquotaIss / 100m, // Converter de percentual para decimal (ex: 2% -> 0.02)
                valor = new
                {
                    servico = notaFiscal.ValorServicos,
                    descontoIncondicionado = notaFiscal.ValorDeducoes,
                    descontoCondicionado = 0m // Se houver campo para isso no futuro
                }
            },
            regimeEspecialTributacao = empresa.RegimeEspecialTributacao ?? "Nenhum",
            optanteSimplesNacional = empresa.OptanteSimplesNacional,
            incentivoFiscal = empresa.IncentivoFiscal
        };

        return payload;
    }

    // Função auxiliar para obter código do município (pode ser melhorada com uma tabela de municípios)
    private string GetCodigoMunicipioByCidadeUF(string cidade, string uf)
    {
        // Por enquanto retorna vazio - idealmente deveria buscar de uma tabela de municípios
        // Ou usar uma API externa para buscar o código IBGE do município
        // Por enquanto, o método acima usa o código do município do prestador como fallback
        return string.Empty;
    }

    private EmitirNotaFiscalResult ConverterRespostaParaResult(JsonElement apiResponse)
    {
        // TODO: Implementar conversão conforme estrutura de resposta da API Nacional
        // Este é um exemplo genérico - ajustar conforme estrutura real
        return new EmitirNotaFiscalResult
        {
            Numero = apiResponse.TryGetProperty("numero", out var num) ? num.GetString() : null,
            CodigoVerificacao = apiResponse.TryGetProperty("codigo_verificacao", out var cod) ? cod.GetString() : null,
            Situacao = (int)SituacaoNotaFiscal.Autorizada,
            XML = apiResponse.TryGetProperty("xml", out var xml) ? xml.GetString() : null,
            JSON = apiResponse.GetRawText()
        };
    }

    private ConsultarNotaFiscalResult ConverterRespostaParaConsultarResult(JsonElement apiResponse)
    {
        // TODO: Implementar conversão conforme estrutura de resposta da API Nacional
        return new ConsultarNotaFiscalResult
        {
            Numero = apiResponse.TryGetProperty("numero", out var num) ? num.GetString() : null,
            CodigoVerificacao = apiResponse.TryGetProperty("codigo_verificacao", out var cod) ? cod.GetString() : null,
            Situacao = (int)SituacaoNotaFiscal.Autorizada,
            XML = apiResponse.TryGetProperty("xml", out var xml) ? xml.GetString() : null,
            JSON = apiResponse.GetRawText()
        };
    }

    private EmitirNotaFiscalResult ConverterRespostaXMLParaResult(string xmlResponse)
    {
        // Converte resposta XML para Result
        // Ajustar conforme estrutura real da resposta da API Nacional
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlResponse);

            var numero = xmlDoc.SelectSingleNode("//Numero")?.InnerText;
            var codigoVerificacao = xmlDoc.SelectSingleNode("//CodigoVerificacao")?.InnerText;

            return new EmitirNotaFiscalResult
            {
                Numero = numero,
                CodigoVerificacao = codigoVerificacao,
                Situacao = (int)SituacaoNotaFiscal.Autorizada,
                XML = xmlResponse,
                JSON = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao converter resposta XML. Retornando resposta completa no campo XML.");
            return new EmitirNotaFiscalResult
            {
                Numero = null,
                CodigoVerificacao = null,
                Situacao = (int)SituacaoNotaFiscal.Autorizada,
                XML = xmlResponse,
                JSON = null
            };
        }
    }

    /// <summary>
    /// Salva o XML assinado em arquivo para análise antes do envio
    /// </summary>
    private async Task SalvarXMLParaAnaliseAsync(string xmlAssinado, int empresaId, NotaFiscalCreateDTO notaFiscal, string sufixo = "")
    {
        try
        {
            if (string.IsNullOrEmpty(xmlAssinado))
            {
                _logger.LogError("XML assinado está vazio ou nulo. Não é possível salvar.");
                throw new ArgumentException("XML assinado não pode ser vazio", nameof(xmlAssinado));
            }

            // Determina o caminho completo da pasta
            var pastaCompleta = Path.IsPathRooted(_pastaXML) 
                ? _pastaXML 
                : Path.Combine(Directory.GetCurrentDirectory(), _pastaXML);

            _logger.LogInformation("Pasta base para XML: {PastaCompleta}", pastaCompleta);

            // Cria subpasta por empresa se necessário
            var pastaEmpresa = Path.Combine(pastaCompleta, $"Empresa_{empresaId}");
            if (!Directory.Exists(pastaEmpresa))
            {
                Directory.CreateDirectory(pastaEmpresa);
                _logger.LogInformation("Pasta da empresa criada: {PastaEmpresa}", pastaEmpresa);
            }

            // Cria subpasta por data (ano/mês)
            var dataAtual = DateTime.Now;
            var pastaData = Path.Combine(pastaEmpresa, $"{dataAtual:yyyy-MM}");
            if (!Directory.Exists(pastaData))
            {
                Directory.CreateDirectory(pastaData);
                _logger.LogInformation("Pasta da data criada: {PastaData}", pastaData);
            }

            // Nome do arquivo: DPS_Empresa{Id}_{DataHora}_{Competencia}{Sufixo}.xml
            var competenciaFormatada = notaFiscal.Competencia.ToString("yyyy-MM");
            var dataFormatada = dataAtual.ToString("yyyyMMdd_HHmmss");
            var sufixoArquivo = !string.IsNullOrEmpty(sufixo) ? $"_{sufixo}" : "";
            var nomeArquivo = $"DPS_Empresa{empresaId}_{dataFormatada}_{competenciaFormatada}{sufixoArquivo}.xml";
            var caminhoCompleto = Path.Combine(pastaData, nomeArquivo);

            _logger.LogInformation("Tentando salvar XML no caminho: {CaminhoCompleto}", caminhoCompleto);

            // Salva o XML
            await File.WriteAllTextAsync(caminhoCompleto, xmlAssinado, Encoding.UTF8);

            // Verifica se o arquivo foi realmente criado
            if (File.Exists(caminhoCompleto))
            {
                var fileInfo = new FileInfo(caminhoCompleto);
                _logger.LogInformation("✅ XML DPS salvo com sucesso! Caminho: {Caminho}, Tamanho: {Tamanho} bytes", 
                    caminhoCompleto, fileInfo.Length);
            }
            else
            {
                _logger.LogError("❌ Arquivo não foi criado após WriteAllTextAsync. Caminho esperado: {Caminho}", caminhoCompleto);
                throw new IOException($"Falha ao criar arquivo XML. O arquivo não existe após a operação de escrita.");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "❌ Erro de permissão ao salvar XML. Verifique as permissões da pasta: {Pasta}", _pastaXML);
            throw;
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError(ex, "❌ Diretório não encontrado ao salvar XML. Pasta: {Pasta}", _pastaXML);
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "❌ Erro de I/O ao salvar XML. Pasta: {Pasta}", _pastaXML);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro inesperado ao salvar XML para análise. Pasta configurada: {Pasta}", _pastaXML);
            throw;
        }
    }

    public async Task<byte[]?> DownloadPDFAsync(string chDPS, string chNFSe, string cnpj, int empresaId)
    {
        // TODO: Implementar download de PDF para API Nacional NFSe
        // Por enquanto, retorna null - a implementação deve seguir a documentação da API Nacional
        _logger.LogWarning("Download de PDF ainda não implementado para API Nacional NFSe");
        return null;
    }
    
    public async Task<ConsultarNotaFiscalResult?> ConsultarStatusAsync(string nsNRec, string cnpj, int empresaId, int? notaFiscalId = null, string? numeroNota = null)
    {
        // TODO: Implementar consulta de status para API Nacional NFSe
        // Por enquanto, retorna null
        _logger.LogWarning("Consulta de status ainda não implementado para API Nacional NFSe");
        return null;
    }
}

