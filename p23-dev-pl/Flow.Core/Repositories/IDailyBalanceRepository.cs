using Flow.Core.Entities;

namespace Flow.Core.Repositories
{
    public interface IDailyBalanceRepository
    {
        Task<DailyBalance> FindDailyBalanceByDay(DateTime date);
    }
}
