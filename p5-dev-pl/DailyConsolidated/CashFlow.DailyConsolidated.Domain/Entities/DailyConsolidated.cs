using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Enums;

namespace CashFlow.DailyConsolidated.Domain.Entities
{
    public class DailyConsolidatedEntity
    {
        public DateOnly Date { get; private set; }
        public decimal DailyResult { get; private set; } = 0;
        public List<Entry> Entries { get; private set; } = new List<Entry>();

        public DailyConsolidatedEntity(DateOnly date, List<Entry> entries)
        {
            Date = date;
            Entries = entries;

            CalculateResult(entries);
        }

        public void CalculateResult(List<Entry> entries)
        {
            foreach (var entry in entries)
            {
                DailyResult += entry.Type == EntryType.Credit ? entry.Value : -entry.Value;
            }
        }
    }
}
