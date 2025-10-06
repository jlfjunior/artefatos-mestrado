using Flow.Core.Security.Tokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Flow.Infra.Tokens
{
    public class TokenGenerate : JWTTokenHandler, ITokenGenerate
    {
        private readonly string _secretKey;
        private readonly int _timeLifeTokenInMinutes;

        public TokenGenerate(string secretKey, int timeLifeTokenInMinutes)
        {
            _secretKey = secretKey;
            _timeLifeTokenInMinutes = timeLifeTokenInMinutes;
        }
        public string GenerateToken(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, userId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_timeLifeTokenInMinutes),
                SigningCredentials = new SigningCredentials(SecurityKey(_secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }
    }
}
