using Challenger.EasyFlow.Domain.Aggregates.CashBoxAggregate;
using Challenger.EasyFlow.Domain.Aggregates.DailyBalanceAggregate;
using Challenger.EasyFlow.Infrastructure.Database.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer.Configurations;

internal sealed class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.ToTable("DailyBalances", DatabaseSchemas.Financial);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OpeningBalance)
            .IsRequired();
        builder.Property(x => x.ClosingBalance)
            .IsRequired();
        builder.Property(x => x.TotalInflow)
            .IsRequired();
        builder.Property(x => x.TotalOutflow)
            .IsRequired();
        builder.HasOne<CashBox>()
            .WithMany()
            .HasForeignKey(x => x.CashBoxId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}