using Hangfire;
using CashFlow.Infrastructure;

namespace CashFlow.Jobs;

public static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage());

        builder.Services.AddHangfireServer();

        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }
}