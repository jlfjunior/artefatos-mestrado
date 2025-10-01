using CashFlow.Application.Common;

namespace CashFlow.Domain.Account
{
    public class AccountDailyBalanceEntity : BaseEntity
    {
        public Guid AccountId { get; set; }

        public DateOnly Date { get; set; }
        public decimal TotalIncoming { get; set; } = 0;
        public decimal TotalOutgoing { get; set; } = 0;
        public decimal BalanceOfDay
        {
            get
            {
                return TotalIncoming - TotalOutgoing;
            }
        }
        public virtual AccountEntity Account { get; set; } = null!;
        public string DateToString() => Date.ToString("yyyy-MM-dd");
    }
}
