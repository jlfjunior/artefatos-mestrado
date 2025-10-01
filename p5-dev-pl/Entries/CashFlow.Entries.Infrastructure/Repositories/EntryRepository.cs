using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Interfaces;

namespace CashFlow.Entries.Infrastructure.Repositories
{
    public class EntryRepository : IEntryRepository
    {
        public List<Entry> Entries { get; set; } = new List<Entry>();

        public async Task<Entry> CreateAsync(Entry entrie)
        {
            Entries.Add(entrie);
            Console.WriteLine($"Entrie created: Date: {entrie.Date}, Value: {entrie.Value}, Description: {entrie.Description}, Type: {entrie.Type}");
            return entrie;
        }
    }
}
