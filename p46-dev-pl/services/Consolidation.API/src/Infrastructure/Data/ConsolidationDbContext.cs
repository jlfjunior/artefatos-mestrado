using Microsoft.EntityFrameworkCore;
using Consolidation.API.Domain.Models;

namespace Consolidation.API.Infrastructure.Data;

public class ConsolidationDbContext : DbContext
{
    public DbSet<Consolidation.API.Domain.Models.Consolidation> Consolidations => Set<Consolidation.API.Domain.Models.Consolidation>();

    public ConsolidationDbContext(DbContextOptions<ConsolidationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Consolidation.API.Domain.Models.Consolidation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Lojista)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.ConsolidationDate)
                .IsRequired();

            entity.Property(e => e.DebitTotal)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.CreditTotal)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.DailyBalance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.ProcessedCount)
                .HasDefaultValue(0);

            entity.Property(e => e.LastUpdatedAt)
                .HasDefaultValue(DateTime.UtcNow);

            // Uma consolidação por lojista por dia.
            entity.HasIndex(e => new { e.ConsolidationDate, e.Lojista }).IsUnique();
        });
    }
}
