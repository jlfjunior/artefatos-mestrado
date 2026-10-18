using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Lancamentos.Api.Auth;

/// <summary>
/// Configuração de JWT com chave simétrica. Em produção a emissão de token
/// ficaria num Identity Provider à parte; aqui mantenho um emissor simples para
/// permitir testar os endpoints protegidos de ponta a ponta.
/// </summary>
public static class JwtConfig
{
    public static (string Issuer, string Audience, string Key) Ler(IConfiguration config)
    {
        var section = config.GetSection("Jwt");
        var issuer = section["Issuer"] ?? "fluxo-caixa";
        var audience = section["Audience"] ?? "fluxo-caixa";
        var key = section["Key"] ?? "chave-de-desenvolvimento-troque-em-producao-please-32+chars";
        return (issuer, audience, key);
    }

    public static void AddJwt(this IServiceCollection services, IConfiguration config)
    {
        var (issuer, audience, key) = Ler(config);
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

    public static string EmitirToken(IConfiguration config, string subject, string papel)
    {
        var (issuer, audience, key) = Ler(config);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(ClaimTypes.Role, papel)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
