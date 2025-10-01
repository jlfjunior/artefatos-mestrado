namespace CashFlow.DailyConsolidated.Application.DTOs
{
    public class DailyConsolidationOutput
    {
        public DateOnly Date { get; set; }
        public decimal DailyResult { get; set; } = 0;
        public List<EntryOutput> Entries { get; set; } = new List<EntryOutput>();
    }

    public class EntryOutput
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }
}
