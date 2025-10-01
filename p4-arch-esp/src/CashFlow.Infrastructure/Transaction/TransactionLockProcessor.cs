using CashFlow.Application.Account;
using CashFlow.Application.Common.Interfaces;
using CashFlow.Application.Transactions;
using CashFlow.Domain.Account;
using CashFlow.Domain.Transactions;
using CashFlow.Infrastructure.Common.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RedLockNet;

namespace CashFlow.Infrastructure.Transaction
{
    public class TransactionLockProcessor : ITransactionLockProcessor
    {
        private const string LockKeyFormat = "transaction-account-lock:{0}";
        private const string LockKeyOptions = nameof(TransactionLockProcessor);

        private readonly RedLockOptions _redLockOptions;

        private readonly IDistributedLockFactory _lockFactory;
        private readonly ICashFlowDbContext _cashFlowDbContext;
        private readonly IAccountCachedRepository _accountCachedRepository;
        private readonly IAccountService _accountService;

        public TransactionLockProcessor(
            IDistributedLockFactory distributedLockFactory,
            ICashFlowDbContext cashFlowDbContext,
            IAccountCachedRepository accountCachedRepository,
            IOptions<RedLockRootOptions> options,
            IAccountService accountService)
        {
            _lockFactory = distributedLockFactory;
            _cashFlowDbContext = cashFlowDbContext;

            _redLockOptions = options.Value.GetOptions(LockKeyOptions);
            _accountCachedRepository = accountCachedRepository;
            _accountService = accountService;
        }

        public async Task DoAsync(Guid accountId, Guid transactionId, CancellationToken cancellationToken)
        {
            await using var _lock =
                await _lockFactory.CreateLockAsync(GetLockKey(accountId),
                    expiryTime: TimeSpan.FromSeconds(_redLockOptions.ExpiryTimeSeconds),
                    waitTime: TimeSpan.FromSeconds(_redLockOptions.WaitTimeSeconds),
                    retryTime: TimeSpan.FromSeconds(_redLockOptions.RetryTimeSeconds));

            if (_lock.IsAcquired)
            {
                var transaction =
                    await _cashFlowDbContext
                        .Transactions
                        .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
                    ?? throw new InvalidOperationException($"Transaction with id {transactionId} not found.");

                // idempotency check
                if (transaction.Status.Equals(ETransactionStatus.Processed))
                    return;

                var account =
                    await _cashFlowDbContext.Accounts
                        .Include(x => x.Balance)
                        .Where(x => x.Id == accountId)
                        .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException($"Account id {transaction.AccountId} not found.");

                var accountDailyBalance =
                    await _accountCachedRepository
                        .GetOrCreateDailyBalanceAsync(accountId, transaction.GetDateCreated());

                _accountService
                    .UpdateBalancesByTransaction(account.Balance, accountDailyBalance, transaction, cancellationToken);

                await _accountCachedRepository
                    .UpdateBalancesAndTransacionAsync(account.Balance, accountDailyBalance, transaction, cancellationToken);
            }
        }

        private static string GetLockKey(Guid userId) => string.Format(LockKeyFormat, userId);


    }
}
