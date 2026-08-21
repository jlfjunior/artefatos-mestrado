using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;
using Microsoft.EntityFrameworkCore;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer.Repositories;

internal sealed class TransactionRepository(EasyFlowContext context) : ITransactionRepository
{
    private readonly DbSet<Transaction> _transactions = context.Set<Transaction>();

    public async ValueTask<int> CountTotalAsync(Guid cashBoxId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _transactions
            .Where(t => t.CashBoxId == cashBoxId && t.CreatedAt == date.ToDateTime(TimeOnly.MinValue))
            .CountAsync(cancellationToken);
    }

    public async ValueTask CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _transactions.AddAsync(transaction, cancellationToken);
    }

    public async ValueTask<IReadOnlyCollection<Transaction>> ListAsync(Guid cashBoxId, int skip, int maxPageSize, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _transactions
            .Where(t => t.CashBoxId == cashBoxId && t.CreatedAt == date.ToDateTime(TimeOnly.MinValue))
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(maxPageSize)
            .ToListAsync(cancellationToken);
    }
}