using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.Consolidation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Persistence;

public sealed class ConsolidationDbContext(DbContextOptions<ConsolidationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<DailyConsolidation> DailyConsolidations => Set<DailyConsolidation>();
    public DbSet<ProcessedLaunchEvent> ProcessedLaunchEvents => Set<ProcessedLaunchEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsolidationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}