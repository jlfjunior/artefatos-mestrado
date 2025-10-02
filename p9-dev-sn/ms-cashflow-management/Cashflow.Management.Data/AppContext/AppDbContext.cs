using Cashflow.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Data.AppContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CashStatement> CashStatements { get; set; }
        public DbSet<ConsolidatedCashBalance> ConsolidatedCashBalances { get; set; }
        public DbSet<ConsolidatedTransactionHistory> ConsolidatedTransactionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsolidatedTransactionHistory>()
                .HasOne(th => th.CashStatement)
                .WithMany(cs => cs.Transactions)
                .HasForeignKey(th => th.CashStatementId);
        }
    }
}
