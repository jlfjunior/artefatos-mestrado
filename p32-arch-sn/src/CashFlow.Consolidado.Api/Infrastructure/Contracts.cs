using CashFlow.Shared;

namespace CashFlow.Consolidado.Api.Infrastructure;

public sealed record ConsolidadoStoragePaths(string ConsolidadoDbPath, string IntegrationDbPath);

public interface IIntegrationEventFeed
{
    Task<IReadOnlyList<IntegrationEnvelope>> ReadNextAsync(long afterSequenceId, int batchSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<IntegrationEnvelope>> ReadAllAsync(CancellationToken cancellationToken);
}

public interface IDailyBalanceProjectionStore
{
    Task<long> GetCheckpointAsync(string consumerName, CancellationToken cancellationToken);
    Task ApplyAsync(string consumerName, IntegrationEnvelope envelope, CancellationToken cancellationToken);
    Task<DailyBalanceSnapshot?> GetAsync(string merchantId, DateOnly businessDate, CancellationToken cancellationToken);
    Task ReplaceAsync(DailyBalanceSnapshot snapshot, CancellationToken cancellationToken);
}

