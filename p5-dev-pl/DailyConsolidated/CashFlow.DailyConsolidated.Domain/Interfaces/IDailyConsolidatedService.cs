using CashFlow.DailyConsolidated.Domain.Entities;

namespace CashFlow.DailyConsolidated.Domain.Interfaces
{
    public interface IDailyConsolidatedService
    {
        public Task<DailyConsolidatedEntity> GetAsync(DateOnly date);
    }
}
