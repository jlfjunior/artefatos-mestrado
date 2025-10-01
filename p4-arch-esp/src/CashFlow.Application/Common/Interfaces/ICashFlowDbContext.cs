using CashFlow.Domain.Account;
using CashFlow.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Application.Common.Interfaces;

public interface ICashFlowDbContext
{
    DbSet<AccountEntity> Accounts { get; set; }
    DbSet<AccountBalanceEntity> AccountBalances { get; set; }
    DbSet<AccountDailyBalanceEntity> AccountDailyBalance { get; set; }    
    DbSet<TransactionEntity> Transactions { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
