using System.Text;
using System.Text.Json;
using CashFlow.BuildingBlocks.IntegrationEvents;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Worker.Messaging;
using CashFlow.Consolidation.Worker.Consumers;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CashFlow.Consolidation.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private string? _consumerTag;

    public Worker(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqOptions> options,
        ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
        _connection = CreateConnection(_options);
        _channel = CreateChannel(_connection, _options);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consolidation worker started. Exchange: {Exchange}, Queue: {Queue}, RoutingKey: {RoutingKey}",
            _options.ExchangeName,
            _options.QueueName,
            _options.RoutingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var integrationEvent = JsonSerializer.Deserialize<LaunchRegisteredIntegrationEvent>(json);

                if (integrationEvent is null)
                {
                    _logger.LogWarning(
                        "Received invalid integration event payload. DeliveryTag: {DeliveryTag}",
                        ea.DeliveryTag);

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation(
                    "Integration event received by worker. LaunchId: {LaunchId}, Type: {Type}, OccurredOnUtc: {OccurredOnUtc}",
                    integrationEvent.LaunchId,
                    integrationEvent.Type,
                    integrationEvent.OccurredOnUtc);

                using var scope = _serviceProvider.CreateScope();

                var launchRegisteredConsumer = scope.ServiceProvider.GetRequiredService<LaunchRegisteredConsumer>();

                var result = await launchRegisteredConsumer.ConsumeAsync(integrationEvent, stoppingToken);

                if (result.IsSuccess)
                {
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                if (IsNonRetryable(result.Error.Type))
                {
                    _logger.LogWarning(
                        "Event processing failed with non-retryable error. LaunchId: {LaunchId}, Code: {Code}",
                        integrationEvent.LaunchId,
                        result.Error.Code);

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
            catch (OperationCanceledException)
            {
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing integration event in worker callback.");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _consumerTag = _channel.BasicConsume(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Consolidation Worker stopped.");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_consumerTag))
            _channel.BasicCancel(_consumerTag);

        _channel.Close();
        _connection.Close();

        return base.StopAsync(cancellationToken);
    }

    private static bool IsNonRetryable(ErrorType errorType)
    {
        return errorType is ErrorType.Business or ErrorType.Validation or ErrorType.NotFound or ErrorType.Conflict;
    }

    private static IConnection CreateConnection(RabbitMqOptions options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost
        };

        return factory.CreateConnection();
    }

    private static IModel CreateChannel(IConnection connection, RabbitMqOptions options)
    {
        var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        channel.QueueDeclare(
            queue: options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueBind(
            queue: options.QueueName,
            exchange: options.ExchangeName,
            routingKey: options.RoutingKey);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        return channel;
    }
}