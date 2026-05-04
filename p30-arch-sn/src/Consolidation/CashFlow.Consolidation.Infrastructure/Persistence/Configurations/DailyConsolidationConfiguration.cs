using CashFlow.Consolidation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Consolidation.Infrastructure.Persistence.Configurations;

public sealed class DailyConsolidationConfiguration : IEntityTypeConfiguration<DailyConsolidation>
{
    public void Configure(EntityTypeBuilder<DailyConsolidation> builder)
    {
        builder.ToTable("daily_consolidations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.TotalCredits)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalDebits)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Balance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(x => x.Date)
            .IsUnique();
    }
}