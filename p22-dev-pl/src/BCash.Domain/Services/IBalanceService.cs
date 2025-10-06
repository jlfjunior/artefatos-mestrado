using BCash.Domain.Entities;

namespace BCash.Domain.Services
{
    public interface IBalanceService
    {
        Task<PagedResponseOffset<Balance>> GetBalancePagedAsync(DateTime initDate, DateTime endDate, int pageNumber, int pageSize);
        Task<Balance> ProcessBalanceAsync(decimal amount, DateTime date, string type);
    }
}