using Commons.Infra.RabbitMQ.Interfaces;
using Transaction.Infrastructure.Messaging;

namespace Transaction.Api.Configurations
{
    public static class MessagingConfiguration
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
            return services;
        }
    }
}
