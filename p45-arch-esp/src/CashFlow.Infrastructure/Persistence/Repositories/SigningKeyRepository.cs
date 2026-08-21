using CashFlow.Infrastructure.Auth;

namespace CashFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de chaves RSA para assinatura JWT.
/// </summary>
/// <param name="connection">Conexão com o banco de dados.</param>
    public sealed class SigningKeyRepository(ScopedDbConnection connection)
    : RepositoryBase(connection), ISigningKeyRepository
{
    /// <summary>
    /// Conta o número de chaves RSA habilitadas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Número de chaves RSA habilitadas.</returns>
    public Task<int> CountEnabledAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM signing_keys WHERE enabled = TRUE";
        return ExecuteScalarAsync<int>(sql, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Insere uma chave RSA.
    /// </summary>
    /// <param name="key">Chave RSA a ser inserida.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public Task InsertAsync(SigningKeyRecord key, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO signing_keys
                (id, key_id, algorithm, key_use, public_modulus_n, public_exponent_e, encrypted_private_key, enabled, created_at)
            VALUES
                (@Id, @KeyId, @Algorithm, @KeyUse, @PublicModulusN, @PublicExponentE, @EncryptedPrivateKey, TRUE, @CreatedAt)
            """;

        return ExecuteCommandAsync(sql, key, cancellationToken);
    }

    /// <summary>
    /// Lista todas as chaves RSA habilitadas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de chaves RSA habilitadas.</returns>
    public Task<IReadOnlyList<SigningKeyRecord>> ListEnabledAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, key_id, algorithm, key_use, public_modulus_n, public_exponent_e, encrypted_private_key, created_at
            FROM signing_keys
            WHERE enabled = TRUE
            ORDER BY created_at ASC
            """;

        return QueryAsync<SigningKeyRecord>(sql, cancellationToken: cancellationToken);
    }
}
