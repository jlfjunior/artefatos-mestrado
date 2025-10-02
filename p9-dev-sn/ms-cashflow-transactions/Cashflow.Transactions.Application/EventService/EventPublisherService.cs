using Cashflow.Transactions.Domain.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Cashflow.Transactions.Application.EventService
{
    public class EventPublisherService<T> : IEventPublisherService<T>
    {
        private readonly RabbitConfig _rabbitConfig;
        private readonly ILogger<EventPublisherService<T>> _logger;

        public EventPublisherService(IOptions<RabbitConfig> options, ILogger<EventPublisherService<T>> logger)
        {
            _rabbitConfig = options.Value;
            _logger = logger;
        }

        public async Task PublishMessageAsync(T message)
        {
            var queueName = GetQueueName();
            if (string.IsNullOrEmpty(queueName))
                throw new InvalidOperationException($"Queue name not defined on event {typeof(T).Name}");

            _logger.LogInformation($"Publishing message to {queueName}...");

            var factory = new ConnectionFactory()
            {
                HostName = _rabbitConfig.Host,
                UserName = _rabbitConfig.User,
                Password = _rabbitConfig.Password
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await channel.BasicPublishAsync(exchange: "", queueName, body);
        }

        private static string GetQueueName()
        {
            var attribute = typeof(T).GetCustomAttribute<EventQueueAttribute>();
            return attribute?.Name;
        }
    }
}
