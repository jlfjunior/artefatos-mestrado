using Flow.Core.Security.Tokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Infra.Tokens
{
    public class TokenValidate : JWTTokenHandler, ITokenValidate
    {
        private readonly string _secretKey;

        public TokenValidate(string secretKey)
        {
            _secretKey = secretKey;
        }
        public Guid GetUserId(string token)
        {
            var principal = TokenValidator(token);
            var userId = principal.FindFirst(ClaimTypes.Sid).Value;

            return Guid.Parse(userId);
        }

        public ClaimsPrincipal TokenValidator(string token)
        {
            var validatorParameters = new TokenValidationParameters
            {
                IssuerSigningKey = SecurityKey(_secretKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = new TimeSpan(0)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.ValidateToken(token, validatorParameters, out _);
        }
    }
}
