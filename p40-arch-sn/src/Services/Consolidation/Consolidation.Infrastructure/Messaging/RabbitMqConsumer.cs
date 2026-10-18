using System.Text;
using System.Text.Json;
using Consolidation.Domain.DTOs;
using Consolidation.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consolidation.Infrastructure.Messaging
{
    public sealed class RabbitMqConsumer : IMessageConsumer, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IProcessEntryCreatedHandler _handler;

        private RabbitMqConsumer(
            IConnection connection,
            IChannel channel,
            IProcessEntryCreatedHandler handler)
        {
            _connection = connection;
            _channel = channel;
            _handler = handler;
        }

        public static async Task<RabbitMqConsumer> CreateAsync(
            string hostName,
            IProcessEntryCreatedHandler handler)
        {
            var factory = new ConnectionFactory { HostName = hostName };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "entry.created",
                durable: true,
                exclusive: false,
                autoDelete: false);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false);

            return new RabbitMqConsumer(connection, channel, handler);
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<EntryCreatedMessage>(json);

                    if (message is not null)
                        await _handler.HandleAsync(message, cancellationToken);

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                }
                catch (Exception)
                {
                    await _channel.BasicNackAsync(
                        eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "entry.created",
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
