using System.Text.Json;
using FinancialBalance.Domain.Accounts;
using FinancialBalance.Domain.Shared;
using FinancialBalance.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FinancialBalance.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly DbContextOptions _ctxOptions;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) => _ctxOptions = options;
    protected AppDbContext(DbContextOptions options) : base(options) => _ctxOptions = options;

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");
        var isInMemory = _ctxOptions.Extensions
            .Any(e => e.GetType().Name.Contains("InMemory", StringComparison.OrdinalIgnoreCase));
        modelBuilder.ApplyConfiguration(new AccountConfiguration(isInMemory));
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PublishDomainEventsToOutbox();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void PublishDomainEventsToOutbox()
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var outbox = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Type = domainEvent.GetType().AssemblyQualifiedName!,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
                };
                OutboxMessages.Add(outbox);
            }
            aggregate.ClearDomainEvents();
        }
    }
}
