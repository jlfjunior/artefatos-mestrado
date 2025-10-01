using CashFlow.Entries.Domain.Enums;

namespace CashFlow.Entries.Application.DTOs
{
    public class CreateEntrieInput
    {
        public string Description { get; set; }
        public decimal Value { get; set; }
        public EntryType Type { get; set; }
    }
}
