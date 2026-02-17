using System.Text.Json;
using NFSe2026.API.Services;

namespace NFSe2026.API.Services;

public class ConsultaCNPJService : IConsultaCNPJService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConsultaCNPJService> _logger;

    public ConsultaCNPJService(HttpClient httpClient, ILogger<ConsultaCNPJService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        // Usando Brasil API - API pública gratuita que consolida dados do governo brasileiro
        _httpClient.BaseAddress = new Uri("https://brasilapi.com.br/api/cnpj/v1/");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NFSe2026-API/1.0");
    }

    public async Task<ConsultaCNPJResult?> ConsultarCNPJAsync(string cnpj)
    {
        try
        {
            // Remove formatação do CNPJ
            cnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();

            if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length != 14)
            {
                throw new ArgumentException("CNPJ inválido");
            }

            var response = await _httpClient.GetAsync(cnpj);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Erro ao consultar CNPJ {CNPJ}: {StatusCode} - {Content}", cnpj, response.StatusCode, errorContent);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            // Brasil API retorna os dados diretamente no objeto principal
            var result = new ConsultaCNPJResult
            {
                CNPJ = jsonDoc.RootElement.TryGetProperty("cnpj", out var cnpjProp) ? cnpjProp.GetString() ?? cnpj : cnpj,
                RazaoSocial = jsonDoc.RootElement.TryGetProperty("razao_social", out var razaoSocial) ? razaoSocial.GetString() ?? "" : "",
                NomeFantasia = jsonDoc.RootElement.TryGetProperty("nome_fantasia", out var nomeFantasia) ? nomeFantasia.GetString() : null,
                SituacaoCadastral = jsonDoc.RootElement.TryGetProperty("descricao_situacao_cadastral", out var situacao) ? situacao.GetString() : null,
                Porte = jsonDoc.RootElement.TryGetProperty("porte", out var porte) ? porte.GetString() : null,
                NaturezaJuridica = jsonDoc.RootElement.TryGetProperty("natureza_juridica", out var natureza) ? natureza.GetString() : null
            };

            // Data de abertura
            if (jsonDoc.RootElement.TryGetProperty("data_inicio_atividade", out var dataInicio))
            {
                var dataStr = dataInicio.GetString();
                if (DateTime.TryParse(dataStr, out var dataAbertura))
                {
                    result.DataAbertura = dataAbertura;
                }
            }

            // Endereço - Brasil API usa estrutura "logradouro" no objeto principal
            if (jsonDoc.RootElement.TryGetProperty("logradouro", out var logradouro))
            {
                result.Endereco = new EnderecoCNPJ
                {
                    Logradouro = logradouro.GetString() ?? "",
                    Numero = jsonDoc.RootElement.TryGetProperty("numero", out var numero) ? numero.GetString() ?? "" : "",
                    Complemento = jsonDoc.RootElement.TryGetProperty("complemento", out var complemento) ? complemento.GetString() : null,
                    Bairro = jsonDoc.RootElement.TryGetProperty("bairro", out var bairro) ? bairro.GetString() ?? "" : "",
                    Cidade = jsonDoc.RootElement.TryGetProperty("municipio", out var municipio) ? municipio.GetString() ?? "" : "",
                    UF = jsonDoc.RootElement.TryGetProperty("uf", out var uf) ? uf.GetString() ?? "" : "",
                    CEP = jsonDoc.RootElement.TryGetProperty("cep", out var cep) ? cep.GetString()?.Replace("-", "") ?? "" : ""
                };
            }

            // Telefone - Brasil API pode ter ddd_telefone_1 (formato: "DDD+NUMERO")
            if (jsonDoc.RootElement.TryGetProperty("ddd_telefone_1", out var telefone1))
            {
                var tel = telefone1.GetString();
                if (!string.IsNullOrEmpty(tel))
                {
                    // Formata telefone se necessário (ex: "11987654321" -> "(11) 98765-4321")
                    result.Telefone = tel;
                }
            }
            
            // Fallback para campo telefone direto
            if (string.IsNullOrEmpty(result.Telefone) && 
                jsonDoc.RootElement.TryGetProperty("telefone", out var telefone))
            {
                result.Telefone = telefone.GetString();
            }

            // Email - geralmente não vem na Brasil API, mas tentamos
            if (jsonDoc.RootElement.TryGetProperty("email", out var email))
            {
                result.Email = email.GetString();
            }

            // Inscrição Estadual - Brasil API retorna em array "inscricoes_estaduais"
            if (jsonDoc.RootElement.TryGetProperty("inscricoes_estaduais", out var inscricoesEstaduais) && 
                inscricoesEstaduais.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var primeiraIE = inscricoesEstaduais.EnumerateArray().FirstOrDefault();
                if (primeiraIE.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                {
                    if (primeiraIE.TryGetProperty("inscricao_estadual", out var ie))
                    {
                        result.InscricaoEstadual = ie.GetString();
                    }
                    else if (primeiraIE.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        result.InscricaoEstadual = primeiraIE.GetString();
                    }
                }
            }
            
            // Tenta também campo direto (fallback)
            if (string.IsNullOrEmpty(result.InscricaoEstadual) && 
                jsonDoc.RootElement.TryGetProperty("inscricao_estadual", out var ieDireto))
            {
                result.InscricaoEstadual = ieDireto.GetString();
            }

            // Inscrição Municipal
            if (jsonDoc.RootElement.TryGetProperty("inscricao_municipal", out var im))
            {
                result.InscricaoMunicipal = im.GetString();
            }

            _logger.LogInformation("CNPJ {CNPJ} consultado com sucesso: {RazaoSocial}", cnpj, result.RazaoSocial);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar CNPJ {CNPJ}", cnpj);
            throw;
        }
    }
}

