using System.Reflection;

namespace EasyCash.Report.Api.Extensions;

public static class ConfigureMediatorExtension
{
    public static IServiceCollection AddCoreApplicationMessaging(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(assemblies);
        });

        return services;
    }
}