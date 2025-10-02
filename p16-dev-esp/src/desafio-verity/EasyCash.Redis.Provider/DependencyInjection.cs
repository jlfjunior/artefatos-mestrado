using EasyCash.Abstractions.Interfaces;
using EasyCash.Redis.Provider.Caching;
using EasyCash.Redis.Provider.Messaging.Pub;
using EasyCash.Redis.Provider.Messaging.Sub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EasyCash.Redis.Provider;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureCachingProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Cache") ??
                                  throw new ArgumentNullException(nameof(configuration));

        services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(connectionString);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectRetry = 5;

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton(typeof(IIntegrationEventSender<>), typeof(RedisPublisherIntegrationEventHandler<>));

        services.AddSingleton<IRedisConsumerFactory, RedisConsumerFactory>();

        return services;
    }
}
