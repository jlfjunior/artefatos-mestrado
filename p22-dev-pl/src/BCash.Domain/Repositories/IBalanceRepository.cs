using BCash.Domain.Entities;

namespace BCash.Domain.Repositories
{
    public interface IBalanceRepository
    {
        Task<Balance> AddAsync(Balance balance);
        Task<Balance> GetByDateAsync(DateTime date);
        Task<PagedResponseOffset<Balance>> GetByDatePagedAsync(DateTime initDate, DateTime endDate, int pageNumber, int pageSize);
        Task<Balance> UpdateAsync(Balance balance);
    }
}
