using CashFlow.Entries.Domain.Entities;

namespace CashFlow.DailyConsolidated.Domain.Interfaces
{
    public interface IEntryRepository
    {
        public Task<IEnumerable<Entry>> GetEntriesByDate(DateOnly date);
    }
}
