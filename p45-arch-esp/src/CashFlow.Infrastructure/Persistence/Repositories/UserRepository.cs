using CashFlow.Application.Abstractions;
using CashFlow.Domain.Entities;
using CashFlow.Shared.Enums;
using CashFlow.Shared.Extensions;

namespace CashFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório Dapper para usuários e referências de acesso.
/// </summary>
/// <param name="connection">Conexão com o banco de dados.</param>
public sealed class UserRepository(ScopedDbConnection connection)
    : RepositoryBase(connection), IUserRepository
{
    /// <summary>
    /// Obtém um usuário por ID.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário ou null se não encontrado.</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, full_name, email, role, enabled, created_at
            FROM users
            WHERE id = @Id AND enabled = TRUE
            """;

        var row = await QuerySingleOrDefaultAsync<UserRow>(sql, new { Id = id }, cancellationToken);
        return row?.ToDomain();
    }

    /// <summary>
    /// Obtém um usuário por email.
    /// </summary>
    /// <param name="email">Email do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário ou null se não encontrado.</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, full_name, email, role, enabled, created_at
            FROM users
            WHERE email = @Email AND enabled = TRUE
            """;

        var row = await QuerySingleOrDefaultAsync<UserRow>(
            sql,
            new { Email = email.Trim().ToLowerInvariant() },
            cancellationToken);

        return row?.ToDomain();
    }

    /// <summary>
    /// Obtém o hash da senha de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Hash da senha ou null se não encontrado.</returns>
    public Task<string?> GetPasswordHashAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT password_hash FROM users WHERE id = @Id AND enabled = TRUE";
        return QuerySingleOrDefaultAsync<string?>(sql, new { Id = id }, cancellationToken);
    }

    /// <summary>
    /// Lista todos os clientes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de clientes.</returns>
    public async Task<IReadOnlyList<User>> ListClientsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, full_name, email, role, enabled, created_at
            FROM users
            WHERE role = @Role AND enabled = TRUE
            ORDER BY full_name
            """;

        var rows = await QueryAsync<UserRow>(
            sql,
            new { Role = UserRole.Client.ToString() },
            cancellationToken);

        return rows.ToReadOnlyList(r => r.ToDomain());
    }

    /// <summary>
    /// Obtém os escopos de autorização de um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de escopos de autorização.</returns>
    public Task<IReadOnlyList<string>> GetScopesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT scope_code
            FROM user_authorization_scopes
            WHERE user_id = @UserId
            ORDER BY scope_code
            """;

        return QueryAsync<string>(sql, new { UserId = userId }, cancellationToken);
    }

    private sealed record UserRow(
        Guid Id,
        string FullName,
        string Email,
        string Role,
        bool Enabled,
        DateTime CreatedAt)
    {
        public User ToDomain() => new()
        {
            Id = Id,
            FullName = FullName,
            Email = Email,
            Role = Enum.Parse<UserRole>(Role, ignoreCase: true),
            Enabled = Enabled,
            CreatedAt = CreatedAt
        };
    }
}
