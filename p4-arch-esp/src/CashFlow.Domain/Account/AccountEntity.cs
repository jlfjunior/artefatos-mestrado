using CashFlow.Application.Common;
using CashFlow.Domain.Transactions;

namespace CashFlow.Domain.Account
{
    public class AccountEntity : BaseEntity
    {
        public Guid IdentityUserId { get; set; }
        public Guid AccountBalanceId { get; set; }
        public required string Name { get; set; }
        
        public virtual AccountBalanceEntity Balance { get; set; } = null!;
        public virtual ICollection<AccountDailyBalanceEntity> DailyBalances { get; set; } = null!;
        public virtual ICollection<TransactionEntity> Transactions { get; set; } = [];
    }
}
