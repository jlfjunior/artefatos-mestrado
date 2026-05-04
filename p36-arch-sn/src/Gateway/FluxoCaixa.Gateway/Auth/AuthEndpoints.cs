using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FluxoCaixa.Gateway.Auth;

public static class AuthEndpoints
{
    // Credenciais fixas para o desafio técnico — em produção usar Identity/Keycloak
    private static readonly Dictionary<string, string> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        { "comerciante@teste.com", "Senha@123" }
    };

    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", (LoginRequest request, IConfiguration config) =>
        {
            if (!_users.TryGetValue(request.Email, out var senha) || senha != request.Senha)
                return Results.Unauthorized();

            var token = GerarToken(request.Email, config);
            return Results.Ok(new { token });
        })
        .AllowAnonymous()
        .WithTags("Auth");
    }

    private static string GerarToken(string email, IConfiguration config)
    {
        var secretKey = config["Jwt:SecretKey"]!;
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        var expiration = int.Parse(config["Jwt:ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "Comerciante"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiration),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Email, string Senha);
