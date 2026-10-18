namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Persistência de clients OAuth2.
/// </summary>
public interface IOAuthClientRepository
{
    /// <summary>
    /// Obtém client OAuth2 pelo identificador público.
    /// </summary>
    /// <param name="clientId">Identificador público do client.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Registro do client ou null se não encontrado.</returns>
    Task<OAuthClientRecord?> GetByPublicClientIdAsync(string clientId, CancellationToken cancellationToken = default);
}
