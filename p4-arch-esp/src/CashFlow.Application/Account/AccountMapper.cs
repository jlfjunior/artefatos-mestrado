using CashFlow.Application.Account.Handlers;
using CashFlow.Application.AccountBalance.Handlers;
using CashFlow.Application.AccountDailyBalance.Handlers;
using CashFlow.Domain.Account;
using Riok.Mapperly.Abstractions;

namespace CashFlow.Application.Account
{
    [Mapper]
    public partial class AccountMapper
    {
        public partial AccountEntity ToAccountEntity(CreateAccountRequest storeRequest);
        public partial GetAccountResponse ToGetAccountResponse(AccountEntity storeEntity);
        public partial GetAccountBalanceResponse ToGetAccountBalanceResponse(AccountBalanceEntity accountBalanceEntity);        
        public partial GetAccountDailyBalanceResponse ToGetDailyBalanceResponse(AccountDailyBalanceEntity entity);
    }
}
