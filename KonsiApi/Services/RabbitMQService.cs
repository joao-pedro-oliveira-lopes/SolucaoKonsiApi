using KonsiApi.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace KonsiApi.Services
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly CacheService _cacheService;
        private readonly BeneficioService _beneficioService;

        public RabbitMQService(IConfiguration configuration, CacheService cacheService, BeneficioService beneficioService)
        {
            var rabbitConfig = configuration.GetSection("RabbitMQ");
            _cacheService = cacheService;
            _beneficioService = beneficioService;
            _connectionFactory = new ConnectionFactory
            {
                HostName = rabbitConfig["Hostname"],
                UserName = rabbitConfig["Username"],
                Password = rabbitConfig["Password"]
            };
        }

        public OperationResult<bool> EnviarCpfParaFila(Cpf cpf, string queueName)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var json = JsonSerializer.Serialize(cpf);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);

                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure("Falha ao enviar CPF para a fila: " + ex.Message);
            }
        }

        public async Task<OperationResult<Cpf>> ConsumirCpfDaFila(string queueName)
        {
            var tcs = new TaskCompletionSource<Cpf>();

            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var cpfConsumido = JsonSerializer.Deserialize<Cpf>(json);

                    // Verifica se o CPF já está no cache
                    var dadosCache = _cacheService.ObterDadosCpf<Beneficio>(cpfConsumido.Numero);
                    if (dadosCache == null)
                    {
                        // Se não estiver no cache, busca os dados de benefício e armazena no cache
                        var dadosBeneficio = await _beneficioService.BuscarDadosBeneficio(cpfConsumido);
                        _cacheService.ArmazenarDadosCpf(cpfConsumido.Numero, dadosBeneficio.Value);
                    }
                    // Se os dados já estiverem no cache, não é necessário processar novamente

                    tcs.SetResult(cpfConsumido); // Sinalizar que o processamento foi concluído
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                return await tcs.Task.ContinueWith(task =>
                    task.Result != null ? OperationResult<Cpf>.Success(task.Result) : OperationResult<Cpf>.Failure("Nenhum CPF disponível na fila."));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex); // Sinaliza a exceção na TaskCompletionSource
                return OperationResult<Cpf>.Failure("Falha ao consumir CPF da fila: " + ex.Message);
            }
        }

        public OperationResult<bool> EnviarCpfParaFila(string cpf, string queueName)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(cpf);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);

                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure("Falha ao enviar CPF para a fila: " + ex.Message);
            }
        }


    }
}
