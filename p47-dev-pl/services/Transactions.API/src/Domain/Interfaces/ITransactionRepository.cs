using Transactions.API.Domain.Models;

namespace Transactions.API.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByDateRangeAsync(DateTime? startDate, DateTime? endDate,
        int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null, 
        CancellationToken cancellationToken = default);
}
