using System.Text.Json;
using Challenger.EasyFlow.Application.Common.EventBus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Challenger.EasyFlow.Infrastructure.EventBus;

internal sealed class RabbitMQEventBus(IConnection connection) : IEventBus
{
    private readonly IConnection _connection = connection;

    public async ValueTask ConsumeAsync<TEvent>(Func<TEvent, CancellationToken, Task> onMessageReceived, string queueName, CancellationToken cancellationToken = default) where TEvent : notnull
    {
        using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel
            .QueueDeclareAsync(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } },
                cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var json = System.Text.Encoding.UTF8.GetString(body);
            var @event = JsonSerializer.Deserialize<TEvent>(json);

            if (@event is not null)
                await onMessageReceived(@event, cancellationToken);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer, cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public async ValueTask PublishAsync<TEvent>(TEvent @event, string queueName, CancellationToken cancellationToken = default) where TEvent : notnull
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel
            .QueueDeclareAsync(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } },
                cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(@event);
        var body = System.Text.Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(string.Empty, queueName, true, body, cancellationToken);
    }
}