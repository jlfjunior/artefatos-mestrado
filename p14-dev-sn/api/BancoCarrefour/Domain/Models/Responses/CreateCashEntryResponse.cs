using static Domain.Enums.Enums;

namespace Domain.Models.Responses
{
    public class CreateCashEntryResponse
    {
        public Guid Id { get; set; }
        public decimal Value { get; set; }
        public DateTime DateCreated { get; set; }
        public EEntryType EntryType { get; set; }
        public ETransactionType TransactionType { get; set; }
    }
}
