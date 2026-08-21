namespace FinancialBalance.Domain.Shared;

public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    Task SaveChangesAsync(CancellationToken ct = default);
}
