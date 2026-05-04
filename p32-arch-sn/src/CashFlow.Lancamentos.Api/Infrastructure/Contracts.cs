using CashFlow.Shared;

namespace CashFlow.Lancamentos.Api.Infrastructure;

public sealed record StoredEntryRecord(LedgerEntry Entry, string RequestHash);

public sealed record LancamentosStoragePaths(string LancamentosDbPath, string IntegrationDbPath);

public interface IEntryCommandStore
{
    Task<StoredEntryRecord?> FindByIdempotencyAsync(string merchantId, string idempotencyKey, CancellationToken cancellationToken);
    Task SaveAsync(LedgerEntry entry, string requestHash, EntryRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}

public interface ILedgerQueryService
{
    Task<IReadOnlyList<RegisterEntryResponse>> ListByBusinessDateAsync(string merchantId, DateOnly businessDate, CancellationToken cancellationToken);
}

public interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);
    Task MarkAsPublishedAsync(Guid eventId, DateTimeOffset publishedAt, CancellationToken cancellationToken);
}

public interface IIntegrationEventBus
{
    Task PublishAsync(OutboxEnvelope envelope, CancellationToken cancellationToken);
}

