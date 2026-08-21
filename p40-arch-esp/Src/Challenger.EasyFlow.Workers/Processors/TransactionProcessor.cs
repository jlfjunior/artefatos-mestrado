using Challenger.EasyFlow.Application.Common.EventBus;
using Challenger.EasyFlow.Application.Features.CashBoxManagement;
using Challenger.EasyFlow.Application.Features.CashBoxManagement.CreateTransaction;

namespace Challenger.EasyFlow.Workers.Processors;

public sealed class TransactionProcessor(IEventBus eventBus, IServiceScopeFactory serviceScopeFactory, ILogger<TransactionProcessor> logger) : BackgroundService
{
    private readonly IEventBus _eventBus = eventBus;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<TransactionProcessor> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TransactionProcessor started at: {time}", DateTimeOffset.Now);

        await _eventBus.ConsumeAsync<CreateTransactionCommand>(async (@event, cancellationToken) =>
        {
            var correlationId = @event.Metadata.TryGetValue("CorrelationId", out var id) ? id : Guid.Empty;
            _logger.LogInformation("Received CreateTransactionCommand with CorrelationId: {CorrelationId} at: {DateTimeUTC}", correlationId, DateTimeOffset.UtcNow);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<CreateTransactionCommandHandler>();

            _ = await handler.HandleAsync(@event, cancellationToken);
        }, QueueNames.TransactionCreated, stoppingToken);

        _logger.LogInformation("TransactionProcessor stopped at: {time}", DateTimeOffset.Now);
    }
}
