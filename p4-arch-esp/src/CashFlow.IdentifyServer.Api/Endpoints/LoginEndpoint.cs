using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CashFlow.IdentifyServer.Api.Endpoints;

public static class LoginEndpoint
{
    public record LoginRequest(string Email, string Password);

    public static WebApplication MapLoginEndpoint(this WebApplication app)
    {
        app.MapPost("/login", async (
            [FromBody]LoginRequest req,
            [FromServices]  UserManager<IdentityUser<Guid>> userManager,
            [FromServices] IOptions<JwtSettings> options) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
                return Results.Unauthorized();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.Value.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: options.Value.Issuer,
                audience: options.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return Results.Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                expiresIn = 3600
            });
        });

        return app;
    }
}
