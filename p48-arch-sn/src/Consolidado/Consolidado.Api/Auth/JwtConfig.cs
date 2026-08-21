using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Consolidado.Api.Auth;

/// <summary>
/// Validação de JWT no read side. Usa a mesma chave simétrica do Lançamentos,
/// então um token emitido lá vale aqui. A emissão fica só no Lançamentos para
/// não duplicar responsabilidade.
/// </summary>
public static class JwtConfig
{
    public static void AddJwt(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection("Jwt");
        var issuer = section["Issuer"] ?? "fluxo-caixa";
        var audience = section["Audience"] ?? "fluxo-caixa";
        var key = section["Key"] ?? "chave-de-desenvolvimento-troque-em-producao-please-32+chars";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = signingKey
                };
            });

        services.AddAuthorization();
    }
}
