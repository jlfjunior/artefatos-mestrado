using CashFlow.Shared;

namespace CashFlow.Consolidado.Api.Infrastructure;

public sealed class IntegrationEventConsumerWorker : BackgroundService
{
    private const string ConsumerName = "daily-balance-consumer";

    private readonly IIntegrationEventFeed _integrationEventFeed;
    private readonly IDailyBalanceProjectionStore _projectionStore;
    private readonly ILogger<IntegrationEventConsumerWorker> _logger;

    public IntegrationEventConsumerWorker(
        IIntegrationEventFeed integrationEventFeed,
        IDailyBalanceProjectionStore projectionStore,
        ILogger<IntegrationEventConsumerWorker> logger)
    {
        _integrationEventFeed = integrationEventFeed;
        _projectionStore = projectionStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var checkpoint = await _projectionStore.GetCheckpointAsync(ConsumerName, stoppingToken);
                var pendingEvents = await _integrationEventFeed.ReadNextAsync(checkpoint, 100, stoppingToken);

                foreach (var integrationEnvelope in pendingEvents)
                {
                    await _projectionStore.ApplyAsync(ConsumerName, integrationEnvelope, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no consumidor do consolidado.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }
}

