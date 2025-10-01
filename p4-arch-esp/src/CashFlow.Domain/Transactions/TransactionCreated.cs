using CashFlow.Domain.Common;

namespace CashFlow.Domain.Transactions
{
    public class TransactionCreated : BaseEvent
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public ETransactionType Type { get; set; }
        public EPaymentMethod PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }
}
