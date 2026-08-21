using CashFlow.Application.Abstractions;
using CashFlow.Contracts;
using CashFlow.Infrastructure.Security;
using CashFlow.Shared.Exceptions;

namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Emissão de tokens OAuth2 via grant type password.
/// </summary>
/// <param name="oauthClients">Repositório de clients OAuth2.</param>
/// <param name="users">Repositório de usuários.</param>
/// <param name="signingKeys">Serviço de assinatura JWT.</param>
public sealed class OAuthTokenService(
    IOAuthClientRepository oauthClients,
    IUserRepository users,
    SigningKeysService signingKeys)
{
    /// <summary>
    /// Emite access token via grant type password.
    /// </summary>
    /// <param name="clientId">Identificador público do client OAuth.</param>
    /// <param name="clientSecret">Secret do client OAuth.</param>
    /// <param name="username">E-mail do usuário.</param>
    /// <param name="password">Senha do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta com access token, expiração e escopos.</returns>
    public async Task<OAuthTokenResponse> IssuePasswordGrantAsync(
        string? clientId,
        string? clientSecret,
        string? username,
        string? password,
        CancellationToken cancellationToken = default)
    {
        var client = await AuthenticateClientAsync(clientId, clientSecret, "password", cancellationToken);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new CodedException("invalid_request", "Os parâmetros username e password são obrigatórios.");
        }

        var user = await users.GetByEmailAsync(username, cancellationToken);
        if (user is null)
        {
            throw new CodedException("invalid_grant", "Credenciais inválidas.");
        }

        var storedHash = await users.GetPasswordHashAsync(user.Id, cancellationToken);
        if (storedHash is null || !PasswordHasher.Verify(password, storedHash))
        {
            throw new CodedException("invalid_grant", "Credenciais inválidas.");
        }

        var scopes = await users.GetScopesAsync(user.Id, cancellationToken);
        var scope = string.Join(' ', scopes);
        var (accessToken, expiresIn) = await signingKeys.SignAccessTokenAsync(
            user.Id,
            user.Role.ToString().ToLowerInvariant(),
            scope,
            client.ClientId,
            cancellationToken);

        return new OAuthTokenResponse(accessToken, expiresIn, scope);
    }

    private async Task<OAuthClientRecord> AuthenticateClientAsync(
        string? clientId,
        string? clientSecret,
        string expectedGrant,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new CodedException("invalid_client", "Client id e secret são obrigatórios.");
        }

        var client = await oauthClients.GetByPublicClientIdAsync(clientId, cancellationToken);
        if (client is null || client.ClientType != "confidential")
        {
            throw new CodedException("invalid_client", "Client OAuth inválido.");
        }

        if (!client.GrantTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(expectedGrant, StringComparer.Ordinal))
        {
            throw new CodedException("unauthorized_client", "Grant type não permitido para este client.");
        }

        if (!PasswordHasher.Verify(clientSecret, client.SecretHash))
        {
            throw new CodedException("invalid_client", "Client OAuth inválido.");
        }

        return client;
    }
}
