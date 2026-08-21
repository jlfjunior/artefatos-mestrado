using CashFlow.Contracts;
using CashFlow.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CashFlow.Api.Controllers;

/// <summary>
/// Emissão de tokens OAuth2.
/// </summary>
/// <param name="tokenService">Serviço de emissão de tokens OAuth2.</param>
[ApiController]
[Route("oauth")]
[Tags("OAuth2")]
public sealed class OAuthTokenController(OAuthTokenService tokenService) : ControllerBase
{
    /// <summary>
    /// Emite access token via grant type password.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Token de acesso ou erro.</returns>
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded", "application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OAuthTokenSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Token(CancellationToken cancellationToken = default)
    {
        var request = await ReadTokenRequestAsync(cancellationToken);
        if (request is null)
        {
            return BadRequest(new ErrorResponse(
                "invalid_request",
                "Content-Type deve ser application/x-www-form-urlencoded ou application/json."));
        }

        if (!string.Equals(request.GrantType, "password", StringComparison.Ordinal))
        {
            return BadRequest(new ErrorResponse(
                "unsupported_grant_type",
                "Somente grant_type=password é suportado."));
        }

        var token = await tokenService.IssuePasswordGrantAsync(
            request.ClientId,
            request.ClientSecret,
            request.Username,
            request.Password,
            cancellationToken);

        return Ok(new OAuthTokenSuccessResponse
        {
            AccessToken = token.AccessToken,
            TokenType = "Bearer",
            ExpiresIn = token.ExpiresIn,
            Scope = token.Scope
        });
    }

    private async Task<TokenRequest?> ReadTokenRequestAsync(CancellationToken cancellationToken)
    {
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            var (clientId, clientSecret) = ResolveClientCredentials(
                form["client_id"].ToString(),
                form["client_secret"].ToString());

            return new TokenRequest(
                form["grant_type"].ToString(),
                clientId,
                clientSecret,
                form["username"].ToString(),
                form["password"].ToString());
        }

        if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            var body = await Request.ReadFromJsonAsync<OAuthTokenRequest>(cancellationToken);
            if (body is null)
            {
                return null;
            }

            var (clientId, clientSecret) = ResolveClientCredentials(body.ClientId, body.ClientSecret);
            return new TokenRequest(
                body.GrantType,
                clientId,
                clientSecret,
                body.Username,
                body.Password);
        }

        return null;
    }

    private (string? ClientId, string? ClientSecret) ResolveClientCredentials(
        string? formClientId,
        string? formClientSecret)
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var encoded = authorization["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':', 2);
            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }
        }

        return (formClientId, formClientSecret);
    }

    private sealed record TokenRequest(
        string GrantType,
        string? ClientId,
        string? ClientSecret,
        string Username,
        string Password);
}
