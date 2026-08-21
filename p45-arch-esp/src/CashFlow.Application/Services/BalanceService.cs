using CashFlow.Application.Abstractions;
using CashFlow.Contracts;
using CashFlow.Application.Exceptions;
using CashFlow.Domain.Entities;
using CashFlow.Shared.Extensions;

namespace CashFlow.Application.Services;

/// <summary>
/// Casos de uso de consulta de saldos (somente leitura).
/// </summary>
/// <param name="repository">Repositório de saldos diários.</param>
/// <param name="currentUser">Contexto de usuário atual.</param>
public sealed class BalanceService(
    IDailyBalanceRepository repository,
    ICurrentUserContext currentUser)
{
    private const int MaxPageSize = 100;

    /// <summary>
    /// Lista saldos diários paginados.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="page">Número da página (base 1).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="from">Data contábil inicial (inclusive).</param>
    /// <param name="to">Data contábil final (inclusive).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta paginada de saldos diários.</returns>
    public async Task<PaginatedBalanceResponse> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        EnsureUserAccess(userId);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, totalCount) = await repository.ListAsync(userId, page, pageSize, from, to, cancellationToken);

        return new PaginatedBalanceResponse(
            items.ToReadOnlyList(Map),
            page,
            pageSize,
            totalCount);
    }

    /// <summary>
    /// Obtém o saldo diário de um usuário em uma data específica.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="date">Data contábil do saldo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta de saldo diário.</returns>
    public async Task<BalanceResponse> GetByDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        EnsureUserAccess(userId);
        var balance = await repository.GetByDateAsync(userId, date, cancellationToken);
        if (balance is not null)
        {
            return Map(balance);
        }

        var previous = await repository.GetLastBeforeDateAsync(userId, date, cancellationToken);
        if (previous is not null)
        {
            return MapCarriedForward(userId, date, previous);
        }

        return Map(DailyBalance.Empty(userId, date));
    }

    /// <summary>
    /// Obtém o saldo diário de um usuário para a data atual.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta de saldo diário.</returns>
    public Task<BalanceResponse> GetTodayAsync(Guid userId, CancellationToken cancellationToken = default) =>
        GetByDateAsync(userId, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

    private void EnsureUserAccess(Guid userId)
    {
        if (currentUser.IsClient && userId != currentUser.UserId)
        {
            throw new ForbiddenException("access_denied", "Cliente só pode consultar os próprios saldos.");
        }
    }

    private static BalanceResponse Map(DailyBalance balance) => new(
        balance.UserId,
        balance.BalanceDate,
        balance.TotalCredits,
        balance.TotalDebits,
        balance.Balance,
        balance.UpdatedAt);

    private static BalanceResponse MapCarriedForward(Guid userId, DateOnly date, DailyBalance previous) => new(
        userId,
        date,
        TotalCredits: 0,
        TotalDebits: 0,
        Balance: previous.Balance,
        previous.UpdatedAt);
}
