namespace EasyCash.Redis.Provider.Messaging.Sub;

public interface IRedisConsumerFactory
{
    Task StartConsumersAsync(CancellationToken cancellationToken);
}