using EasyCash.Redis.Provider.Messaging.Sub;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyCash.Background.Jobs.IntegrationEventsConsumer;

public class IntegrationEventConsumerJob : BackgroundService
{
    private readonly IRedisConsumerFactory _consumerFactory;
    private readonly ILogger<IntegrationEventConsumerJob> _logger;

    public IntegrationEventConsumerJob(IRedisConsumerFactory consumerFactory, ILogger<IntegrationEventConsumerJob> logger)
    {
        _consumerFactory = consumerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Redis Streams Consumption Service Started");
        await _consumerFactory.StartConsumersAsync(stoppingToken);
    }
}