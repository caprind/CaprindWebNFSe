using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NFSe2026.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiService> _logger;
    private readonly string _baseUrl;

    public ApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5215";
        
        // Configura headers padrão que não mudam por requisição
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, $"{_baseUrl}/api/{endpoint}");
        
        // Adiciona token de autorização se existir
        var httpContext = _httpContextAccessor.HttpContext;
        var token = httpContext?.Session?.GetString("JWTToken");
        
        if (string.IsNullOrEmpty(token))
        {
            // Não loga aviso para auth/login pois é esperado não ter token
            if (!endpoint.StartsWith("auth/"))
            {
                _logger.LogWarning("TOKEN VAZIO - Método: {Method}, Endpoint: {Endpoint}, HttpContext null: {IsNull}, Session null: {SessionNull}", 
                    method, endpoint, httpContext == null, httpContext?.Session == null);
            }
        }
        else
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("Token adicionado ao request: {Method} {Endpoint}", method, endpoint);
        }
        
        return request;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var request = CreateRequest(HttpMethod.Get, endpoint);
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Log para debug
                _logger.LogInformation("GET {Endpoint} retornou {StatusCode} - Conteúdo: {ContentLength} bytes", 
                    endpoint, response.StatusCode, content?.Length ?? 0);
                
                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("GET {Endpoint} retornou resposta vazia", endpoint);
                    return default;
                }
                
                try
                {
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Erro ao deserializar resposta do GET {Endpoint}. Conteúdo: {Content}", endpoint, content?.Substring(0, Math.Min(content.Length, 500)));
                    return default;
                }
            }
            
            // Log erro detalhado
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("GET {Endpoint} retornou {StatusCode}. Resposta: {ErrorContent}", 
                endpoint, response.StatusCode, errorContent);
            
            // Se for erro 401, lança exceção específica para tratamento de autenticação
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");
            }
            
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer GET {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            var request = CreateRequest(HttpMethod.Post, endpoint);
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            
            // Tenta extrair mensagem de erro do corpo da resposta
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("POST {Endpoint} retornou {StatusCode}. Resposta completa: {ErrorContent}", endpoint, response.StatusCode, errorContent);
            
            if (!string.IsNullOrEmpty(errorContent))
            {
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (errorObj != null && errorObj.ContainsKey("error"))
                    {
                        var errorMessage = errorObj["error"]?.ToString();
                        _logger.LogWarning("POST {Endpoint} retornou {StatusCode}: {Error}", endpoint, response.StatusCode, errorMessage);
                        
                        // Lança exceção com mensagem de erro para ser capturada pelo controller
                        throw new HttpRequestException($"Erro {response.StatusCode}: {errorMessage}");
                    }
                }
                catch (JsonException)
                {
                    // Se não conseguir deserializar, usa a resposta completa
                    _logger.LogWarning("Não foi possível deserializar erro JSON. Resposta: {ErrorContent}", errorContent);
                }
            }
            
            _logger.LogWarning("POST {Endpoint} retornou {StatusCode}. Resposta: {ErrorContent}", endpoint, response.StatusCode, errorContent);
            throw new HttpRequestException($"Erro ao fazer POST {endpoint}: {response.StatusCode}. Resposta: {errorContent}");
        }
        catch (HttpRequestException)
        {
            throw; // Re-lança HttpRequestException
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer POST {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> PutAsync(string endpoint, object? data = null)
    {
        try
        {
            _logger.LogInformation("PUT {Endpoint} - Iniciando requisição...", endpoint);
            var request = CreateRequest(HttpMethod.Put, endpoint);
            if (data != null)
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                    WriteIndented = false
                };
                var json = JsonSerializer.Serialize(data, options);
                _logger.LogInformation("PUT {Endpoint} - JSON enviado: {Json}", endpoint, json);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            else
            {
                _logger.LogWarning("PUT {Endpoint} - Nenhum dado fornecido", endpoint);
            }
            
            _logger.LogInformation("PUT {Endpoint} - Enviando requisição HTTP...", endpoint);
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("PUT {Endpoint} - Status: {StatusCode}, Resposta: {Response}", endpoint, response.StatusCode, responseContent);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PUT {Endpoint} falhou com status {StatusCode}. Resposta: {Response}", endpoint, response.StatusCode, responseContent);
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Erro HTTP ao fazer PUT {Endpoint}: {Message}", endpoint, httpEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer PUT {Endpoint}: {Message}", endpoint, ex.Message);
            throw;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            var request = CreateRequest(HttpMethod.Put, endpoint);
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseContent))
                {
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("PUT {Endpoint} retornou {StatusCode}. Resposta: {ErrorContent}", 
                endpoint, response.StatusCode, errorContent);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");
            }
            
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer PUT {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var request = CreateRequest(HttpMethod.Delete, endpoint);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer DELETE {Endpoint}", endpoint);
            throw;
        }
    }
}

