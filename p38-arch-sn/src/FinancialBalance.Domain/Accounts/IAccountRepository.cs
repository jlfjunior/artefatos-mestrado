using FinancialBalance.Domain.Shared;

namespace FinancialBalance.Domain.Accounts;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task<(IReadOnlyList<Account> Items, int TotalCount)> ListAsync(
        bool? isActive, int page, int pageSize, CancellationToken ct = default);
    Task<Account?> GetByIdWithTransactionAsync(Guid accountId, Guid transactionId, CancellationToken ct = default);
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> ListTransactionsAsync(
        Guid accountId,
        TransactionType? type,
        TransactionCategory? category,
        TransactionStatus? status,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
