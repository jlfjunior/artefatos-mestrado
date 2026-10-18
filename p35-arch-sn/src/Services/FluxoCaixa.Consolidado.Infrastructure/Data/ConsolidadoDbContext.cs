using FluxoCaixa.Consolidado.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FluxoCaixa.Consolidado.Infrastructure.Data;

public class ConsolidadoDbContext : DbContext
{
    public ConsolidadoDbContext(DbContextOptions<ConsolidadoDbContext> options) : base(options) { }

    public DbSet<ConsolidadoDiario> Consolidados => Set<ConsolidadoDiario>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConsolidadoDiario>(b =>
        {
            b.HasKey(x => x.Data);
            b.Property(x => x.TotalCreditos).HasPrecision(18, 2);
            b.Property(x => x.TotalDebitos).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProcessedEvent>(b =>
        {
            b.HasKey(x => x.EventId);
        });
    }
}
