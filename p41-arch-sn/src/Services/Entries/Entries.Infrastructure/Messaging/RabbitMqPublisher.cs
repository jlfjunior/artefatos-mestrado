using System.Text;
using System.Text.Json;
using Entries.Domain.Interfaces;
using RabbitMQ.Client;

namespace Entries.Infrastructure.Messaging
{
    public sealed class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        private RabbitMqPublisher(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }

        public static async Task<RabbitMqPublisher> CreateAsync(string hostName)
        {
            var factory = new ConnectionFactory { HostName = hostName };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "entry.exchange",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            await channel.QueueDeclareAsync(
                queue: "entry.created",
                durable: true,
                exclusive: false,
                autoDelete: false);

            await channel.QueueBindAsync(
                queue: "entry.created",
                exchange: "entry.exchange",
                routingKey: "entry.created");

            return new RabbitMqPublisher(connection, channel);
        }

        public async Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await _channel.BasicPublishAsync(
                exchange: "entry.exchange",
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
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
