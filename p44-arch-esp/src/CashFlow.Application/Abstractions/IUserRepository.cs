using CashFlow.Domain.Entities;

namespace CashFlow.Application.Abstractions;

/// <summary>
/// Porta de persistência de usuários autenticáveis.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtém um usuário por ID.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário ou null se não encontrado.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um usuário por email.
    /// </summary>
    /// <param name="email">Email do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário ou null se não encontrado.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o hash da senha de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Hash da senha ou null se não encontrado.</returns>
    Task<string?> GetPasswordHashAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista clientes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de clientes.</returns>
    Task<IReadOnlyList<User>> ListClientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os escopos de um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de escopos.</returns>
    Task<IReadOnlyList<string>> GetScopesAsync(Guid userId, CancellationToken cancellationToken = default);
}
