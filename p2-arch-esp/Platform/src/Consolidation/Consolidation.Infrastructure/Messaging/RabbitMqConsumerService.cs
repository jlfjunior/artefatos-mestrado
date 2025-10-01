using Microsoft.Extensions.Hosting;

namespace Consolidation.Infrastructure.Messaging;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly RabbitMqEventConsumer _consumer;

    public RabbitMqConsumerService(RabbitMqEventConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.StartConsuming();
        return Task.CompletedTask;
    }
}
