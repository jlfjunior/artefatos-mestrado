using CashFlow.DailyConsolidated.Domain.Interfaces;
using CashFlow.DailyConsolidated.Domain.Mocks;
using CashFlow.Entries.Domain.Entities;

namespace CashFlow.DailyConsolidated.Infrastructure.Repositories
{
    public class EntryRepository : IEntryRepository
    {
        public async Task<IEnumerable<Entry>> GetEntriesByDate(DateOnly date)
        {
            return EntriesMocks.GetEntriesMocks()
                .Where(e => e.Date.Date == date.ToDateTime(TimeOnly.MinValue).Date)
                .ToList();
        }
    }
}
