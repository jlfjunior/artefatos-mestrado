using CashFlow.Consolidation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Consolidation.Infrastructure.Persistence.Configurations;

public sealed class ProcessedLaunchEventConfiguration : IEntityTypeConfiguration<ProcessedLaunchEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedLaunchEvent> builder)
    {
        builder.ToTable("processed_launch_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.LaunchId)
            .IsRequired();

        builder.Property(x => x.ProcessedOnUtc)
            .IsRequired();

        builder.HasIndex(x => x.LaunchId)
            .IsUnique();
    }
}
