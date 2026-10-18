using Challenger.EasyFlow.Domain.Aggregates.CashBoxAggregate;
using Challenger.EasyFlow.Domain.Aggregates.MerchantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer.Configurations;

internal sealed class CashBoxConfiguration : IEntityTypeConfiguration<CashBox>
{
    public void Configure(EntityTypeBuilder<CashBox> builder)
    {
        builder.ToTable("CashBoxes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Balance)
            .IsRequired();
        builder.Property(x => x.OpenedAt)
            .IsRequired();
        builder.Property(x => x.ClosedAt);
        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(x => x.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
