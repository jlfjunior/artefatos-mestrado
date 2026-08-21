using CashFlow.Application.Abstractions;
using CashFlow.Contracts;
using CashFlow.Application.Exceptions;
using CashFlow.Shared.Enums;
using CashFlow.Shared.Extensions;

namespace CashFlow.Application.Services;

/// <summary>
/// Casos de uso de escrita e leitura de usuários clientes.
/// </summary>
/// <param name="users">Repositório de usuários.</param>
/// <param name="currentUser">Contexto de usuário atual.</param>
public sealed class ClientService(IUserRepository users, ICurrentUserContext currentUser)
{
    /// <summary>
    /// Lista todos os clientes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de clientes.</returns>
    public async Task<IReadOnlyList<ClientResponse>> ListClientsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsEmployee)
        {
            throw new ForbiddenException("employee_required", "Somente funcionários podem listar clientes.");
        }

        var items = await users.ListClientsAsync(cancellationToken);
        return items.ToReadOnlyList(Map);
    }

    /// <summary>
    /// Obtém um cliente por ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Cliente ou null se não encontrado.</returns>
    public async Task<ClientResponse?> GetClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (currentUser.IsClient && id != currentUser.UserId)
        {
            throw new ForbiddenException("access_denied", "Cliente só pode consultar o próprio perfil.");
        }

        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null || user.Role != UserRole.Client)
        {
            return null;
        }

        return Map(user);
    }

    /// <summary>
    /// Obtém o cliente atual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Cliente atual ou null se não encontrado.</returns>
    public Task<ClientResponse?> GetCurrentClientAsync(CancellationToken cancellationToken = default) =>
        GetClientAsync(currentUser.UserId, cancellationToken);

    private static ClientResponse Map(Domain.Entities.User user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.CreatedAt);
}
