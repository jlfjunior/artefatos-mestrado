using Cashflow.Transactions.Application.EventService;

namespace Cashflow.Transactions.Api.Bootstrap
{
    public static class EventConfiguration
    {
        public static IServiceCollection ConfigureEvent(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitConfig>(configuration.GetSection("RabbitMQ"));

            services.AddScoped(typeof(IEventPublisherService<>), typeof(EventPublisherService<>));

            return services;
        }
    }
}
