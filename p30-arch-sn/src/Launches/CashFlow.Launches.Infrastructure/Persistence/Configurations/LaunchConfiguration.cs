using CashFlow.Launches.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Launches.Infrastructure.Persistence.Configurations;

public sealed class LaunchConfiguration : IEntityTypeConfiguration<Launch>
{
    public void Configure(EntityTypeBuilder<Launch> builder)
    {
        builder.ToTable("launches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();
    }
}