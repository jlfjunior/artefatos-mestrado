using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Enums;

namespace CashFlow.Entries.Domain.Interfaces
{
    public interface IEntryService
    {
        public Task<Entry> CreateAsync(decimal value, string description, EntryType type, DateTime? date = null);
    }
}
