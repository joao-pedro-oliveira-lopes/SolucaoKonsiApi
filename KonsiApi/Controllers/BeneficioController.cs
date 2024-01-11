using Microsoft.AspNetCore.Mvc;
using KonsiApi.Models;
using KonsiApi.Services;

namespace KonsiApi.Controllers
{
    public class BeneficioController : ControllerBase
    {
        private readonly BeneficioService _beneficioService;

        public BeneficioController(BeneficioService beneficioService)
        {
            _beneficioService = beneficioService;
        }

        [HttpGet("consulta-beneficios")]
        public async Task<IActionResult> BuscarBeneficiosPorCpf([FromQuery] string cpf)
        {
            var cpfObj = new Cpf { Numero = cpf };
            var resultado = await _beneficioService.BuscarBeneficiosPorCpf(cpfObj);

            if (resultado.Succeeded)
            {
                return Ok(resultado.Value);
            }

            return BadRequest(resultado.Error);
        }
    }
}
