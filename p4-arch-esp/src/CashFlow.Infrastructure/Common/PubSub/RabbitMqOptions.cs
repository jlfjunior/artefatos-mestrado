using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CashFlow.Infrastructure.Common.PubSub
{
    public class RabbitMqOptions
    {
        public static string SectionName = "RabbitMq";
        public required Uri ConnectionString { get; set; }
    }

    public class ConfigureRabbitMqOptions(IConfiguration configuration) : IConfigureOptions<RabbitMqOptions>
    {
        public void Configure(RabbitMqOptions options)
        {
            options.ConnectionString = new Uri(configuration.GetConnectionString(RabbitMqOptions.SectionName)!);
        }
    }
}
