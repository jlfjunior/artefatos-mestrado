using CashFlow.Entries.Domain.Entities;

namespace CashFlow.Entries.Domain.Interfaces
{
    public interface IEntryRepository
    {
        public Task<Entry> CreateAsync(Entry entrie);
    }
}
