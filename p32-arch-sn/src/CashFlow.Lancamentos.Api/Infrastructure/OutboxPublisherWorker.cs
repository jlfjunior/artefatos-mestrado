using CashFlow.Shared;

namespace CashFlow.Lancamentos.Api.Infrastructure;

public sealed class OutboxPublisherWorker : BackgroundService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IOutboxRepository outboxRepository,
        IIntegrationEventBus integrationEventBus,
        TimeProvider timeProvider,
        ILogger<OutboxPublisherWorker> logger)
    {
        _outboxRepository = outboxRepository;
        _integrationEventBus = integrationEventBus;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar eventos da outbox.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        var pendingMessages = await _outboxRepository.GetPendingAsync(100, cancellationToken);
        if (pendingMessages.Count == 0)
        {
            return;
        }

        foreach (var message in pendingMessages)
        {
            await _integrationEventBus.PublishAsync(message, cancellationToken);
            await _outboxRepository.MarkAsPublishedAsync(message.EventId, _timeProvider.GetUtcNow(), cancellationToken);
        }
    }
}

