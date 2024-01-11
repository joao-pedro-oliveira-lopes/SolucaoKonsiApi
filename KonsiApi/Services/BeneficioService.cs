using KonsiApi.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace KonsiApi.Services
{
    public class BeneficioService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalApiConfig _apiConfig;
        private readonly ElasticsearchService _elasticsearchService;

        public BeneficioService(HttpClient httpClient, IOptions<ExternalApiConfig> apiConfig, ElasticsearchService elasticsearchService)
        {
            _httpClient = httpClient;
            _apiConfig = apiConfig.Value;
            _elasticsearchService = elasticsearchService;
        }

        private async Task<OperationResult<string>> AutenticarNaApiExterna()
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_apiConfig.BaseUrl}/api/v1/token",
                new { username = _apiConfig.Username, password = _apiConfig.Password });

            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<string>.Failure("Falha na autenticação com a API externa: " + response.ReasonPhrase);
            }

            var token = await response.Content.ReadFromJsonAsync<Token>();
            return token != null ? OperationResult<string>.Success(token.Value) : OperationResult<string>.Failure("Token nulo recebido da API externa.");
        }

        public async Task<OperationResult<IEnumerable<Beneficio>>> BuscarBeneficiosPorCpf(Cpf cpf)
        {
            if (!cpf.IsValid())
            {
                return OperationResult<IEnumerable<Beneficio>>.Failure("CPF inválido.");
            }

            var autenticacaoResultado = await AutenticarNaApiExterna();

            if(!autenticacaoResultado.Succeeded)
            {
                return OperationResult<IEnumerable<Beneficio>>.Failure("Falha na autenticação: " + autenticacaoResultado.Error);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacaoResultado.Value);

            var response = await _httpClient.GetAsync($"{_apiConfig.BaseUrl}/api/v1/inss/consulta-beneficios?cpf={cpf.Numero}");

            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<IEnumerable<Beneficio>>.Failure("Erro ao buscar benefícios: " + response.ReasonPhrase);
            }

            var beneficios = await response.Content.ReadFromJsonAsync<IEnumerable<Beneficio>>();
            return OperationResult<IEnumerable<Beneficio>>.Success(beneficios);
        }

        public async Task<OperationResult<IEnumerable<Beneficio>>> BuscarDadosBeneficio(Cpf cpf)
        {
            var autenticacaoResultado = await AutenticarNaApiExterna();
            if (!autenticacaoResultado.Succeeded)
            {
                return OperationResult<IEnumerable<Beneficio>>.Failure("Falha na autenticação: " + autenticacaoResultado.Error);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacaoResultado.Value);

            var urlBase = _apiConfig.BaseUrl;
            var endpoint = $"/api/v1/inss/consulta-beneficios?cpf={cpf.Numero}";

            try
            {
                var response = await _httpClient.GetAsync(urlBase + endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var beneficios = JsonSerializer.Deserialize<IEnumerable<Beneficio>>(content);
                    foreach (var beneficio in beneficios)
                    {
                        await _elasticsearchService.IndexBeneficioAsync(beneficio);
                    }
                    return OperationResult<IEnumerable<Beneficio>>.Success(beneficios);
                }
                else
                {
                    return OperationResult<IEnumerable<Beneficio>>.Failure("Erro ao buscar benefícios: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<Beneficio>>.Failure("Falha ao buscar dados de benefício: " + ex.Message);
            }
        }
    }

}
