using KonsiApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KonsiApi.Services
{
    
    public class AutenticacaoService
    {
        private readonly JwtConfig _jwtConfig;

        public AutenticacaoService(IOptions<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig.Value;
        }

        public OperationResult<Token> GerarToken(Usuario usuario)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.Name, usuario.Username)
                // Outras claims, se necessário
            }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return OperationResult<Token>.Success(new Token
                {
                    Value = tokenHandler.WriteToken(token),
                    ExpiryDate = tokenDescriptor.Expires.Value
                });
            }
            catch (Exception ex)
            {
                return OperationResult<Token>.Failure("Falha ao gerar token: " + ex.Message);
            }
        }


    }
}
