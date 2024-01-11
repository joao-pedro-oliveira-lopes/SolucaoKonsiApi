
using Serilog;

namespace KonsiApi.Services
{
    public class RabbitMQBackgroundService : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly string _queueName;

        public RabbitMQBackgroundService(RabbitMQService rabbitMQService, IConfiguration configuration)
        {
            _rabbitMQService = rabbitMQService;
            _queueName = configuration["RabbitMQConfig:Queues:BeneficiosConsulta"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _rabbitMQService.ConsumirCpfDaFila(_queueName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro ao consumir CPF da fila.");
                }

                await Task.Delay(1000, stoppingToken); // Intervalo regular entre as tentativas de consumo
            }
        }

    }
}
