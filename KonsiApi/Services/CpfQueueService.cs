using KonsiApi.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using KonsiApi.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KonsiApi.Services
{
    public class CpfQueueService
    {
        private readonly ICpfRepository _cpfRepository;
        private readonly RabbitMQService _rabbitMQService;
        private readonly string _queueName;

        public CpfQueueService(ICpfRepository cpfRepository, RabbitMQService rabbitMQService, string queueName)
        {
            _cpfRepository = cpfRepository;
            _rabbitMQService = rabbitMQService;
            _queueName = queueName;
        }

        public async Task EnqueueCpfsAsync()
        {
            IEnumerable<Cpf> cpfs = await _cpfRepository.GetAllCpfsAsync();

            foreach (var cpf in cpfs)
            {
                
                if (cpf.Processado)
                {
                    Log.Information("CPF {CpfNumero} já foi processado anteriormente.", cpf.Numero);
                    continue;
                }

                var resultado = _rabbitMQService.EnviarCpfParaFila(cpf.Numero, _queueName);
                if (resultado.Succeeded)
                {
                    
                    cpf.Processado = true;
                    await _cpfRepository.UpdateCpfAsync(cpf);
                }
                else
                {
                    Log.Error("Falha ao enviar CPF {CpfNumero} para a fila: {Error}", cpf.Numero, resultado.Error);
                }
            }
        }
    }
}
