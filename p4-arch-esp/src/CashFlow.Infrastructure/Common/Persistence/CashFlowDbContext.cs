using CashFlow.Application.Common.Interfaces;
using CashFlow.Domain.Account;
using CashFlow.Domain.Transactions;
using CashFlow.Infrastructure.Common.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.Common.Persistence;

public class CashFlowDbContext : DbContext, ICashFlowDbContext
{
    readonly UpdateTrackerEntitiesInterceptor updateTrackerEntitiesInterceptor= new();


    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<AccountBalanceEntity> AccountBalances { get; set; }
    public DbSet<AccountDailyBalanceEntity> AccountDailyBalance { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }

    public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(updateTrackerEntitiesInterceptor);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashFlowDbContext).Assembly);

        modelBuilder.UseStringEnums();
    }
}

