using KonsiApi.Models;
using KonsiApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace KonsiApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly AutenticacaoService _autenticacaoService;

        public AutenticacaoController(AutenticacaoService autenticacaoService)
        {
            _autenticacaoService = autenticacaoService;
        }

        [HttpPost("token")]
        public IActionResult GerarToken([FromBody] Usuario usuario)
        {
            var resultado = _autenticacaoService.GerarToken(usuario);

            if (resultado.Succeeded)
            {
                return Ok(resultado.Value);
            }

            return BadRequest(resultado.Error);
        }

    }
}
