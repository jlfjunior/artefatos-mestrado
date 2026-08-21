using CashFlow.Contracts;
using CashFlow.Infrastructure.Auth;
using CashFlow.Infrastructure.Configuration;
using CashFlow.Shared.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
namespace CashFlow.Api.Controllers;

/// <summary>
/// Endpoints de descoberta OpenID Connect.
/// </summary>
/// <param name="signingKeys">Serviço de chaves de assinatura.</param>
/// <param name="options">Opções de autenticação OAuth2.</param>
[ApiController]
[Route(".well-known")]
[Tags("OAuth2")]
[Produces("application/json")]
public sealed class WellKnownController(
    SigningKeysService signingKeys,
    IOptions<OAuthOptions> options) : ControllerBase
{
    /// <summary>
    /// Retorna o documento de configuração OpenID Connect da API.
    /// </summary>
    /// <returns>Metadados de descoberta (issuer, endpoints, escopos e claims suportados).</returns>
    [HttpGet("openid-configuration")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(OpenIdConfigurationResponse), StatusCodes.Status200OK)]
    public ActionResult<OpenIdConfigurationResponse> OpenIdConfiguration()
    {
        var issuer = options.Value.Issuer.TrimEnd('/');
        return Ok(new OpenIdConfigurationResponse
        {
            Issuer = issuer,
            TokenEndpoint = $"{issuer}/oauth/token",
            JwksUri = $"{issuer}/.well-known/jwks.json",
            GrantTypesSupported = ["password"],
            IdTokenSigningAlgValuesSupported = ["RS256"],
            ScopesSupported = AuthorizationScopes.Supported,
            ClaimsSupported = ["sub", "role", "scope", "token_use", "client_id"]
        });
    }

    /// <summary>
    /// Retorna o conjunto de chaves públicas JWT (JWKS) para validação de tokens.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Chaves RSA públicas habilitadas no authorization server.</returns>
    [HttpGet("jwks.json")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(JsonWebKeySetResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<JsonWebKeySetResponse>> Jwks(CancellationToken cancellationToken = default)
    {
        var jwks = await signingKeys.GetJwksAsync(cancellationToken);
        return Ok(jwks);
    }
}
