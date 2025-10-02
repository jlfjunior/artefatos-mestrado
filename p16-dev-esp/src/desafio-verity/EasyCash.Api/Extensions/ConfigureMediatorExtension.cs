using System.Reflection;
using EasyCash.Api.Behaviors;

namespace EasyCash.Api.Extensions;

public static class ConfigureMediatorExtension
{
    public static IServiceCollection AddCoreApplicationMessaging(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(assemblies);

            services.AddApplicationGenericBehaviors(configuration);
        });

        return services;
    }
}