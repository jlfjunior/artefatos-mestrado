using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControleLancamentos.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ControleLancamentos.Api.Controllers;

/// <summary>
/// Operações de autenticação para geração de token de teste.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/autenticacao")]
public class AutenticacaoController(IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Gera um JWT para uso em testes da API.
    /// </summary>
    /// <param name="request">Dados do usuário de teste.</param>
    /// <returns>Token JWT no esquema Bearer.</returns>
    [HttpPost("token-teste")]
    [ProducesResponseType(typeof(GerarTokenTesteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<GerarTokenTesteResponse> GerarTokenTeste([FromBody] GerarTokenTesteRequest? request)
    {
        var jwtSection = configuration.GetRequiredSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer não configurado.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience não configurado.");
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret não configurado.");

        if (secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret deve ter no mínimo 32 caracteres.");
        }

        var usuario = string.IsNullOrWhiteSpace(request?.Usuario) ? "usuario-teste" : request.Usuario.Trim();
        var expiracaoMinutos = int.TryParse(jwtSection["ExpiracaoMinutos"], out var valorExpiracao) ? valorExpiracao : 120;
        var expiraEm = DateTime.UtcNow.AddMinutes(expiracaoMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credentials);

        var tokenSerializado = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new GerarTokenTesteResponse(tokenSerializado, "Bearer", expiraEm));
    }
}
