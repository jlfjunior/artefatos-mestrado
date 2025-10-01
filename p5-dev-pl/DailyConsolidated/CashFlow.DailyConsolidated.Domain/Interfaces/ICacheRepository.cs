using CashFlow.DailyConsolidated.Domain.Entities;

namespace CashFlow.DailyConsolidated.Domain.Interfaces
{
    public interface ICacheRepository
    {
        public Task<DailyConsolidatedEntity?> GetDailyConsolidatedByDate(DateOnly date);

        public Task AddDailyConsolidation(DailyConsolidatedEntity dailyConsolidated);
    }
}
