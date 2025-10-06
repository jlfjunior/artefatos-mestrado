using Flow.Application.MessageBrokers;
using Flow.Application.UseCases.Login;
using Flow.Application.UseCases.Transaction;
using Flow.Core.Security.Tokens;
using Flow.Infra.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Application
{
    public static class DependencyInjectionTransactionApplication
    {
        public static void AddDependencyInjectionTransactionApplication(this IServiceCollection services, IConfiguration configuration)
        {
            AddUseCases(services);
            AddKafka(services, configuration);
        }

        public static void AddUseCases(IServiceCollection services)
        {
            services.AddScoped<IExecuteTransactionUseCase, ExecuteTransactionUseCase>();
            services.AddScoped<IDoLoginUseCase, DoLoginUseCase>();
        }

        public static void AddKafka(IServiceCollection services, IConfiguration configuration)
        {
            var kafkaSection = configuration.GetSection("Kafka");
            services.AddScoped<IKafkaProducer>(option => new KafkaProducer(kafkaSection["BootstrapServers"], kafkaSection["Topic"]));
        }
    }
}
