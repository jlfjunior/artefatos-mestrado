using CashFlow.Domain.Transactions;

namespace CashFlow.Domain.Account
{
    public interface IAccountService
    {
        void UpdateBalancesByTransaction(
            AccountBalanceEntity accountBalance,
            AccountDailyBalanceEntity accountDailyBalance,
            TransactionEntity transaction,
            CancellationToken cancellationToken);
    }

    public class AccountService : IAccountService
    {
        public void UpdateBalancesByTransaction(
            AccountBalanceEntity accountBalance,
            AccountDailyBalanceEntity accountDailyBalance,
            TransactionEntity transaction,
            CancellationToken cancellationToken)
        {
            var totalAmountTransaction = transaction.TotalAmount;

            switch (transaction.Type)
            {
                case ETransactionType.Incoming:
                    accountBalance.CurrentTotal += totalAmountTransaction;
                    accountDailyBalance.TotalIncoming += totalAmountTransaction;
                    break;

                case ETransactionType.Outgoing:
                    accountBalance.CurrentTotal -= totalAmountTransaction;
                    accountDailyBalance.TotalOutgoing += totalAmountTransaction;
                    break;
                default:
                    throw new NotImplementedException($"Transaction type {transaction.Type} not implemented.");
            }
            transaction.MarkAsProcessed();
        }
    }
}
