using CashFlow.Application.Common;
using CashFlow.Domain.Transactions;

namespace CashFlow.Domain.Account
{
    public class AccountBalanceEntity : BaseEntity
    {
        public Guid AccountId { get; set; }
        public decimal CurrentTotal { get; set; } = 0;
        public virtual AccountEntity Account { get; set; } = null!;

        public bool UpdateTotalByTransaction(TransactionEntity transaction)
        {
            switch (transaction.Type)
            {
                case ETransactionType.Incoming:
                    CurrentTotal += transaction.TotalAmount;
                    transaction.MarkAsProcessed();
                    return true;

                case ETransactionType.Outgoing:
                    // Por não se trata de uma conta corrente, não validamos se o saldo é suficiente.
                    CurrentTotal -= transaction.TotalAmount;
                    transaction.MarkAsProcessed();
                    return true;

                default:
                    throw new NotImplementedException($"Transaction type {transaction.Type} not implemented.");
            }
        }
    }
}
