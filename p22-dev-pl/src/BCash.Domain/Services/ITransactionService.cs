using BCash.Domain.Entities;

namespace BCash.Domain.Services
{
    public interface ITransactionService
    {
        Task CancelTransactionAsync(Guid id);

        Task<Transaction> GetTransactionAsync(Guid id);

        Task<PagedResponseOffset<Transaction>> GetTransactionPagedAsync(DateTime initDate, DateTime endDate, int pageNumber, int pageSize);

        Task<Transaction> ProcessTransactionAsync(Transaction transaction);
    }
}