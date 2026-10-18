using Challenger.EasyFlow.Domain.Aggregates.MerchantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer.Configurations;

internal sealed class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(4);
        builder.Property(x => x.Timezone)
            .HasMaxLength(255);
    }
}