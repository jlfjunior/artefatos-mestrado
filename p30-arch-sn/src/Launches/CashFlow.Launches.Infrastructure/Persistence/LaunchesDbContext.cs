using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.Launches.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Launches.Infrastructure.Persistence;

public sealed class LaunchesDbContext(DbContextOptions<LaunchesDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Launch> Launches => Set<Launch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LaunchesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}