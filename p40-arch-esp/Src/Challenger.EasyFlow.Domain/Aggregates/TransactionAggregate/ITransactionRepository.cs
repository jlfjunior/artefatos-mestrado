namespace Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;

public interface ITransactionRepository
{
    ValueTask CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    ValueTask<int> CountTotalAsync(Guid cashBoxId, DateOnly date, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyCollection<Transaction>> ListAsync(Guid cashBoxId, int skip, int maxPageSize, DateOnly date, CancellationToken cancellationToken = default);
}