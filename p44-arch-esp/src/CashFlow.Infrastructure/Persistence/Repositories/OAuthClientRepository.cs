using CashFlow.Infrastructure.Auth;

namespace CashFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de clients OAuth2 confidenciais.
/// </summary>
/// <param name="connection">Conexão com o banco de dados.</param>
public sealed class OAuthClientRepository(ScopedDbConnection connection)
    : RepositoryBase(connection), IOAuthClientRepository
{
    /// <summary>
    /// Obtém um client OAuth2 por ID público.
    /// </summary>
    /// <param name="clientId">ID público do client OAuth2.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Client OAuth2 ou null se não encontrado.</returns>
    public Task<OAuthClientRecord?> GetByPublicClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT c.id, c.client_id, c.client_type, c.grant_types, s.secret_hash
            FROM oauth_clients c
            INNER JOIN oauth_client_secrets s ON s.oauth_client_id = c.id
            WHERE c.client_id = @ClientId AND c.enabled = TRUE
            """;

        return QuerySingleOrDefaultAsync<OAuthClientRecord>(sql, new { ClientId = clientId }, cancellationToken);
    }
}
