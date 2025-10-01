using CashFlow.Domain.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Infrastructure.Account
{
    public class AccountDailyBalanceEntityConfiguration : IEntityTypeConfiguration<AccountDailyBalanceEntity>
    {
        public void Configure(EntityTypeBuilder<AccountDailyBalanceEntity> builder)
        {
            builder.ToTable("AccountDailyBalance");
        }
    }
}
