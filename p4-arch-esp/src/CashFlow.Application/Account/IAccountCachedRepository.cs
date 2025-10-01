using CashFlow.Application.Account.Handlers;
using CashFlow.Application.AccountBalance.Handlers;
using CashFlow.Application.AccountDailyBalance.Handlers;
using CashFlow.Domain.Account;
using CashFlow.Domain.Transactions;

namespace CashFlow.Application.Account
{
    public interface IAccountCachedRepository
    {
        Task CreateAccountAsync(AccountEntity accountEntity, CancellationToken cancellationToken);
        Task UpdateBalancesAndTransacionAsync(AccountBalanceEntity balance, AccountDailyBalanceEntity dailyBalance,
            TransactionEntity transaction, CancellationToken cancellationToken);
        Task<GetAccountBalanceResponse?> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken);
        Task<IEnumerable<GetAccountResponse>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<AccountDailyBalanceEntity> GetOrCreateDailyBalanceAsync(Guid accountId, DateOnly date);
        Task<GetAccountDailyBalanceResponse?> GetDailyBalanceAsync(Guid accountId, DateOnly date, CancellationToken cancellationToken);
    }
}