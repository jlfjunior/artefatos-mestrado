using static Domain.Enums.Enums;

namespace Domain.Models.Requests
{
    public class CreateCashEntryRequest
    {
        public CreateCashEntryRequest()
        {
            
        }
        public decimal Value { get; set; }
        public EEntryType EntryType { get; set; }
        public ETransactionType TransactionType { get; set; }
    }
}
