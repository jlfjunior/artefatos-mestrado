using EasyCash.Abstractions;
using EasyCash.Abstractions.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace EasyCash.Redis.Provider.Messaging.Pub;

public sealed class RedisPublisherIntegrationEventHandler<TEvent> : IIntegrationEventSender<TEvent> where TEvent : IntegrationEventBase
{
    private readonly IConnectionMultiplexer _redis;

    public RedisPublisherIntegrationEventHandler(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task Send(TEvent message, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var messageJson = JsonSerializer.Serialize(message);

        var streamName = $"stream:{message.EndpointName}";
        var messageId = await db.StreamAddAsync(streamName, new NameValueEntry[]
        {
            new NameValueEntry("message", messageJson)
        });
    }
}

