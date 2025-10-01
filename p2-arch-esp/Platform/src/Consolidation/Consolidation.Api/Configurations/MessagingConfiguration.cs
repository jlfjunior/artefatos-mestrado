using Commons.Infra.RabbitMQ.Handlers;
using Consolidation.Application.Handlers;
using Consolidation.Infrastructure.Messaging;

namespace Consolidation.Api.Configurations;

public static class MessagingConfiguration
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMqEventConsumer>();
        services.AddSingleton<ICreatedTransactionEventHandler, CreatedTransactionEventHandler>();
        services.AddHostedService<RabbitMqConsumerService>();

        return services;
    }
}
