using KonsiApi.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace KonsiApi.Services
{
    public class CacheService
    {
        private readonly IDatabase _cache;

        public CacheService(IConfiguration configuration)
        {
            var redisConfig = configuration.GetSection("RedisConfig");
            var redis = ConnectionMultiplexer.Connect(redisConfig["ConnectionString"]);
            _cache = redis.GetDatabase();
        }

        public OperationResult<bool> SalvarDadosNoCache<T>(string chave, T dados)
        {
            try
            {
                var json = JsonSerializer.Serialize(dados);
                _cache.StringSet(chave, json);
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure("Falha ao salvar dados no cache: " + ex.Message);
            }
        }

        public OperationResult<T> ObterDadosCpf<T>(string cpf)
        {
            try
            {
                var valor = _cache.StringGet(cpf);
                if (valor.IsNullOrEmpty)
                {
                    return OperationResult<T>.Failure("Nenhum cpf encontrado no cache.");
                }

                var dados = JsonSerializer.Deserialize<T>(valor);
                return dados != null ? OperationResult<T>.Success(dados) : OperationResult<T>.Failure("Erro ao deserializar os dados.");
            }
            catch (Exception ex)
            {
                return OperationResult<T>.Failure("Falha ao obter dados do cache: " + ex.Message);
            }
        }

        public OperationResult<bool> ArmazenarDadosCpf(string chave, IEnumerable<Beneficio> dadosBeneficio)
        {
            try
            {
                var json = JsonSerializer.Serialize(dadosBeneficio);
                _cache.StringSet(chave, json);
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure("Falha ao salvar dados no cache: " + ex.Message);
            }
        }

    }
}
