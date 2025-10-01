using CashFlow.Application.Account;
using CashFlow.Application.Account.Handlers;
using CashFlow.Application.AccountBalance.Handlers;
using CashFlow.Application.AccountDailyBalance.Handlers;
using CashFlow.Application.Common.Interfaces;
using CashFlow.Domain.Account;
using CashFlow.Domain.Transactions;
using CashFlow.Infrastructure.Common.Cache;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace CashFlow.Infrastructure.Account;

public class AccountCachedRepository : CacheRepositoryBase, IAccountCachedRepository
{
    private readonly AccountMapper _accountMapper = new();

    public AccountCachedRepository(HybridCache hybridCache, ICashFlowDbContext cashFlowDbContext)
        : base(cashFlowDbContext, hybridCache)
    {
    }

    public async Task CreateAccountAsync(AccountEntity accountEntity, CancellationToken cancellationToken)
    {
        await _cashFlowDbContext.Accounts.AddAsync(accountEntity, cancellationToken);
        var accountBalanceEntity = new AccountBalanceEntity()
        {
            AccountId = accountEntity.Id,
            Account = accountEntity,
            CurrentTotal = 0,
        };
        await _cashFlowDbContext.AccountBalances.AddAsync(accountBalanceEntity, cancellationToken);
        await _cashFlowDbContext.SaveChangesAsync(cancellationToken);

        //var accounts = (await _cashFlowDbContext.Accounts
        //        .Where(a => a.IdentityUserId == accountEntity.IdentityUserId)
        //        .ToListAsync(cancellationToken))
        //        .Select(x => _accountMapper.ToGetAccountResponse(x));
        //var userAccountsKey = AccountCacheKeys.AccountsByUserKey(accountEntity.IdentityUserId);
        //await SetToCacheAsync(userAccountsKey, accounts, cancellationToken);

        //var accountBalanceResponse = _accountMapper.ToGetAccountBalanceResponse(accountBalanceEntity);
        //var balanceAccountKey = AccountCacheKeys.AccountBalanceKey(accountBalanceEntity.Id);
        //await SetToCacheAsync(balanceAccountKey, accountBalanceResponse, cancellationToken);
    }

    public async Task UpdateBalancesAndTransacionAsync(AccountBalanceEntity balance,
        AccountDailyBalanceEntity dailyBalance, TransactionEntity transaction, CancellationToken cancellationToken)
    {
        _cashFlowDbContext.AccountBalances.Update(balance);
        _cashFlowDbContext.AccountDailyBalance.Update(dailyBalance);
        _cashFlowDbContext.Transactions.Update(transaction);
        await _cashFlowDbContext.SaveChangesAsync(cancellationToken);

        var balanceKey = AccountCacheKeys.AccountBalanceKey(balance.AccountId);

        var accountBalanceResponse = _accountMapper.ToGetAccountBalanceResponse(balance);
        await SetToCacheAsync(balanceKey, accountBalanceResponse, cancellationToken);

        var dailyBalanceKey = AccountCacheKeys.DailyBalanceByAccount(dailyBalance.Date, dailyBalance.AccountId);
        var dailyBalanceResponse = _accountMapper.ToGetDailyBalanceResponse(dailyBalance);
        await SetToCacheAsync(dailyBalanceKey, dailyBalanceResponse, cancellationToken);
    }

    public async Task<GetAccountBalanceResponse?> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var key = AccountCacheKeys.AccountBalanceKey(accountId);

        return await _hybridCache.GetOrCreateAsync(key, async token =>
        {
            var accountBalanceEntity =
                await _cashFlowDbContext.AccountBalances
                    .Where(b => b.AccountId == accountId)
                    .FirstOrDefaultAsync(cancellationToken);

            if (accountBalanceEntity is null)
                return null;

            return _accountMapper.ToGetAccountBalanceResponse(accountBalanceEntity);
        },
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(DefaultCacheDurationInMinutes),
            LocalCacheExpiration = TimeSpan.FromSeconds(DefaultLocalCacheDurationInSeconds)
        },
        cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<GetAccountResponse>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var key = AccountCacheKeys.AccountsByUserKey(userId);

        return await _hybridCache.GetOrCreateAsync(key, async token =>
        {
            IEnumerable<GetAccountResponse> accounts =
                (await _cashFlowDbContext.Accounts
                .Where(b => b.IdentityUserId == userId)
                .ToListAsync(cancellationToken))
                .Select(a => _accountMapper.ToGetAccountResponse(a));

            return accounts;
        },
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(DefaultCacheDurationInMinutes),
            LocalCacheExpiration = TimeSpan.FromSeconds(DefaultLocalCacheDurationInSeconds)
        },
        cancellationToken: cancellationToken);
    }

    public async Task<AccountDailyBalanceEntity> GetOrCreateDailyBalanceAsync(Guid accountId, DateOnly date)
    {
        var dailyBalance =
            await _cashFlowDbContext.AccountDailyBalance.FirstOrDefaultAsync(x => x.AccountId == accountId && x.Date == date);

        if (dailyBalance is null)
        {
            dailyBalance = new AccountDailyBalanceEntity()
            {
                AccountId = accountId,
                Date = date
            };

            await _cashFlowDbContext.AccountDailyBalance.AddAsync(dailyBalance);
            await _cashFlowDbContext.SaveChangesAsync();
        }

        return dailyBalance;
    }

    public async Task<GetAccountDailyBalanceResponse?> GetDailyBalanceAsync(Guid accountId, DateOnly date,
        CancellationToken cancellationToken)
    {
        var dailyBalanceKey = AccountCacheKeys.DailyBalanceByAccount(date, accountId);

        return await _hybridCache.GetOrCreateAsync(dailyBalanceKey, async token =>
        {
            var accountBalanceEntity =
                await GetOrCreateDailyBalanceAsync(accountId, date);

            if (accountBalanceEntity is null)
                return null;

            return _accountMapper.ToGetDailyBalanceResponse(accountBalanceEntity);
        },
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(DefaultCacheDurationInMinutes),
            LocalCacheExpiration = TimeSpan.FromSeconds(DefaultLocalCacheDurationInSeconds)
        },
        cancellationToken: cancellationToken);
    }
}