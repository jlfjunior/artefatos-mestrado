using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Enums;

namespace CashFlow.DailyConsolidated.Domain.Mocks
{
    public static class EntriesMocks
    {
        public static List<Entry> GetEntriesMocks()
        {
            return new List<Entry>
            {
                new Entry(new DateTime(2025, 7, 27), 5000.00m, "Salary deposit", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 27), 1200.00m, "Rent payment", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 27), 300.00m, "Utilities", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 28), 800.00m, "Freelance payment", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 28), 250.00m, "Grocery shopping", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 29), 2000.00m, "Client payment", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 29), 100.00m, "Internet bill", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 29), 80.00m, "Phone bill", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 30), 1500.00m, "Investment return", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 30), 400.00m, "Car maintenance", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 30), 150.00m, "Restaurant", EntryType.Debt)
            };
        }
    }
}
