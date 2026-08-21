namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Persistência de chaves de assinatura JWT.
/// </summary>
public interface ISigningKeyRepository
{
    /// <summary>
    /// Conta chaves habilitadas no pool.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Quantidade de chaves habilitadas.</returns>
    Task<int> CountEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere uma nova chave de assinatura.
    /// </summary>
    /// <param name="key">Registro da chave a persistir.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    Task InsertAsync(SigningKeyRecord key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista chaves habilitadas para assinatura e JWKS.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Chaves habilitadas ordenadas para uso.</returns>
    Task<IReadOnlyList<SigningKeyRecord>> ListEnabledAsync(CancellationToken cancellationToken = default);
}
