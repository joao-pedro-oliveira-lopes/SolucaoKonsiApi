using Elasticsearch.Net;
using Nest;
using System;
using KonsiApi.Models;
using System.Threading.Tasks;

namespace KonsiApi.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticClient _client;

        public ElasticsearchService(IConfiguration configuration)
        {
            var settings = new ConnectionSettings(new Uri(configuration["ElasticsearchConfig:Uri"]))
                .BasicAuthentication(configuration["ElasticsearchConfig:Username"], configuration["ElasticsearchConfig:Password"]);

            _client = new ElasticClient(settings);
        }

        public async Task IndexBeneficioAsync(Beneficio beneficio)
        {
            var response = await _client.IndexDocumentAsync(beneficio);
            if (!response.IsValid)
            {
                throw new InvalidOperationException($"Erro ao indexar no Elasticsearch: {response.OriginalException.Message}");
            }
        }
    }
}
