using static Domain.Enums.Enums;

namespace Domain.Entities
{
    public class CashEntry : BaseEntity
    {        
        public decimal Value { get; set; }
        public EEntryType EntryType { get; set; }
        public ETransactionType TransactionType { get; set; }
    }
}
