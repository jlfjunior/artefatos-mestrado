using System.Text;
using System.Text.Json;
using CashFlow.BuildingBlocks.IntegrationEvents;
using CashFlow.Launches.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CashFlow.Launches.Infrastructure.Messaging;

public sealed class RabbitMqIntegrationEventPublisher : ILaunchIntegrationEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqIntegrationEventPublisher> _logger;

    public RabbitMqIntegrationEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqIntegrationEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        _channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey);
    }

    public Task PublishRegisteredAsync(
        LaunchRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(integrationEvent);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Launch registered event published to RabbitMQ. LaunchId: {LaunchId}, Exchange: {Exchange}, RoutingKey: {RoutingKey}",
            integrationEvent.LaunchId,
            _options.ExchangeName,
            _options.RoutingKey);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}