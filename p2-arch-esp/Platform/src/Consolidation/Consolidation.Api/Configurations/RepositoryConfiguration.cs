using Consolidation.Application.Interfaces.Repository;
using Consolidation.Infrastructure.Data;

namespace Consolidation.Api.Configurations;

public static class RepositoryConfiguration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IConsolidateRepository, ConsolidateRepository>();
        return services;
    }
}
