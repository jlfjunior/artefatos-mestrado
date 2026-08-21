using Consolidation.Domain.Entities;

namespace Consolidation.Domain.Repositories
{
    public interface IDailyBalanceRepository
    {
        Task<DailyBalance?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DailyBalance>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
        Task AddAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default);
        Task UpdateAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default);
    }
}
