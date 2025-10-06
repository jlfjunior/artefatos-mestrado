using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Infra.Tokens
{
    public abstract class JWTTokenHandler
    {
        protected static SymmetricSecurityKey SecurityKey(string secretKey)
        {
            var bytes = Encoding.UTF8.GetBytes(secretKey);

            return new SymmetricSecurityKey(bytes);
        }
    }
}
