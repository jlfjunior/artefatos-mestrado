using Cashflow.Management.Application.EventService;

namespace Cashflow.Management.Api.Bootstrap
{
    public static class EventConfiguration
    {
        public static IServiceCollection ConfigureEvent(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitConfig>(configuration.GetSection("RabbitMQ"));

            return services;
        }
    }
}
