using BCash.Domain.Entities;

namespace BCash.Domain.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction> AddAsync(Transaction transaction);

        Task DeleteAsync(Guid id);

        Task<PagedResponseOffset<Transaction>> GetByDatePagedAsync(DateTime initDate, DateTime endDate, int pageNumber, int pageSize);

        Task<Transaction> GetByIdAsync(Guid id);
    }
}
