using CashFlow.Consolidado.Api.Infrastructure;
using CashFlow.Lancamentos.Api.Infrastructure;
using CashFlow.Shared;

namespace CashFlow.Tests;

public sealed class FrozenTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FrozenTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}

public sealed class InMemoryEntryCommandStore : IEntryCommandStore
{
    private readonly Dictionary<(string MerchantId, string IdempotencyKey), StoredEntryRecord> _items = new();

    public List<EntryRegisteredIntegrationEvent> SavedEvents { get; } = [];

    public Task<StoredEntryRecord?> FindByIdempotencyAsync(string merchantId, string idempotencyKey, CancellationToken cancellationToken)
    {
        _items.TryGetValue((merchantId, idempotencyKey), out var record);
        return Task.FromResult(record);
    }

    public Task SaveAsync(
        LedgerEntry entry,
        string requestHash,
        EntryRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        _items[(entry.MerchantId, entry.IdempotencyKey)] = new StoredEntryRecord(entry, requestHash);
        SavedEvents.Add(integrationEvent);
        return Task.CompletedTask;
    }
}

public sealed class InMemoryIntegrationEventFeed : IIntegrationEventFeed
{
    public List<IntegrationEnvelope> Events { get; } = [];

    public Task<IReadOnlyList<IntegrationEnvelope>> ReadNextAsync(long afterSequenceId, int batchSize, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<IntegrationEnvelope>>(Events.Where(x => x.SequenceId > afterSequenceId).Take(batchSize).ToArray());

    public Task<IReadOnlyList<IntegrationEnvelope>> ReadAllAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<IntegrationEnvelope>>(Events.OrderBy(x => x.SequenceId).ToArray());
}

public sealed class InMemoryProjectionStore : IDailyBalanceProjectionStore
{
    public DailyBalanceSnapshot? Snapshot { get; private set; }

    public Task<long> GetCheckpointAsync(string consumerName, CancellationToken cancellationToken)
        => Task.FromResult(0L);

    public Task ApplyAsync(string consumerName, IntegrationEnvelope envelope, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task<DailyBalanceSnapshot?> GetAsync(string merchantId, DateOnly businessDate, CancellationToken cancellationToken)
        => Task.FromResult(Snapshot);

    public Task ReplaceAsync(DailyBalanceSnapshot snapshot, CancellationToken cancellationToken)
    {
        Snapshot = snapshot;
        return Task.CompletedTask;
    }
}

