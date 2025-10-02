using EasyCash.Abstractions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace EasyCash.Redis.Provider.Messaging.Sub;

public class RedisConsumerFactory : IRedisConsumerFactory
{
    private readonly IIntegrationConsumerInitializer _consumerInitializer;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RedisConsumerFactory> _logger;
    private readonly IConfiguration _configuration;

    public RedisConsumerFactory(
        IIntegrationConsumerInitializer consumerInitializer,
        IConnectionMultiplexer redis,
        IServiceProvider serviceProvider,
        ILogger<RedisConsumerFactory> logger
        , IConfiguration configuration)
    {
        _consumerInitializer = consumerInitializer;
        _redis = redis;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartConsumersAsync(CancellationToken cancellationToken)
    {
        foreach (var consumerType in _consumerInitializer.Consumers)
        {
            var streamName = $"stream:{consumerType.EndpointName}";

            _logger.LogInformation($"Initializing consumer for stream: {streamName}");

            await StartAsync(streamName, consumerType, cancellationToken)
                .ContinueWith(async (t) =>
                {
                    if (t.IsFaulted)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                        await StartAsync(streamName, consumerType, cancellationToken);
                    }

                    if (t.IsCompleted)
                        _logger.LogError("Consumer stopped unexpectedly!");
                }, cancellationToken)
                .ConfigureAwait(false);
        }

        await Task.CompletedTask;
    }

    private async Task StartAsync(string streamName, IntegrationEventBase consumerType, CancellationToken cancellationToken) =>
        await Task.Run(async () => await StartConsumerForStreamAsync(streamName, consumerType.GetType(), cancellationToken), cancellationToken);

    private async Task StartConsumerForStreamAsync(string streamName, Type eventType, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var consumerGroup = GetConsumerGroupName();
        var consumerName = GetConsumerName();

        try
        {
            await CreateConsumerGroupIfNotExists(streamName, db, consumerGroup);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var pendingMessages = await db.StreamReadGroupAsync(
                        streamName,
                        consumerGroup,
                        consumerName,
                        count: 1,
                        noAck: false);

                    if (pendingMessages.Length > 0)
                    {
                        _logger.LogInformation($"Processing {pendingMessages.Length} pending messages from {streamName}");
                        await ProcessStreamEntriesAsync(pendingMessages, streamName, eventType, db, consumerGroup);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing stream messages {streamName}");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Fatal error in stream consumer {streamName}");
        }
    }

    private string GetConsumerGroupName()
    {
        return _configuration["Redis:Stream:ConsumerGroup"] ?? throw new ArgumentException();
    }

    private static string GetConsumerName()
    {
        return $"consumer-{Environment.MachineName}-{Guid.NewGuid().ToString().Substring(0, 8)}";
    }

    private async Task CreateConsumerGroupIfNotExists(string streamName, IDatabase db, string? consumerGroup)
    {
        try
        {
            await db.StreamCreateConsumerGroupAsync(streamName, consumerGroup, StreamPosition.Beginning);
            _logger.LogInformation($"Consumer group created for the stream: {streamName}");
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            _logger.LogInformation($"Consumer group already exists for the stream: {streamName}");
        }
    }

    private async Task ProcessStreamEntriesAsync(
        StreamEntry[] entries,
        string streamName,
        Type eventType,
        IDatabase db,
        string consumerGroup)
    {
        foreach (var entry in entries)
        {
            try
            {
                var json = entry.Values.FirstOrDefault().Value;
                if (json.IsNull)
                {
                    _logger.LogWarning($"Empty message in stream {streamName}");
                    continue;
                }

                var integrationEvent = (IntegrationEventBase)JsonSerializer.Deserialize(json.ToString(), eventType)!;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                    await publisher.Publish(integrationEvent!, CancellationToken.None);

                    _logger.LogInformation($"Event processed: {eventType.Name}, ID: {integrationEvent!.Id}");
                }

                await db.StreamAcknowledgeAsync(streamName, consumerGroup, entry.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar mensagem {entry.Id} do stream {streamName}");
            }
        }
    }
}