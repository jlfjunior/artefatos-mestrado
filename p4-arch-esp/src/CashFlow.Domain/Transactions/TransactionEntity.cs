using CashFlow.Application.Common;
using CashFlow.Domain.Account;

namespace CashFlow.Domain.Transactions
{
    public class TransactionEntity : BaseEntity
    {
        public Guid AccountId { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; } = null;
        public ETransactionType Type { get; set; }
        public ETransactionStatus Status { get; set; } = ETransactionStatus.Pending;
        public EPaymentMethod PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; } = 0;
        public string? Notes { get; set; }

        public virtual AccountEntity Account { get; set; } = null!;

        public void MarkAsProcessed()
        {
            Status = ETransactionStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
        }
        
        public DateOnly GetDateCreated() => DateOnly.FromDateTime(CreatedAt.UtcDateTime);
    }
}
