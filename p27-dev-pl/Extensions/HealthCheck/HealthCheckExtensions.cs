using Foundation.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Extensions.HealthCheck
{
    public static  class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddHealthChecks()
                .AddMongoDb(appSettings.MongoUrl)
                .AddRabbitMQ(rabbitConnectionString:$"amqp://{appSettings.MessageBroker.User}:{appSettings.MessageBroker.Password}@{appSettings.MessageBroker.Host}/");
            return services;
        }

        public static void UsePrometheusHealthChecks(this WebApplication app)
        {
            app.UseHealthChecksPrometheusExporter("/_health-metrics");

        }
    }
}
