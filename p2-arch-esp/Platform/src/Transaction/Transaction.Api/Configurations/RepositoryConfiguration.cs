using Transaction.Application.Interfaces.Repository;
using Transaction.Infrastructure.Data;

namespace Transaction.Api.Configurations;

public static class RepositoryConfiguration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IMovementRepository, MovementRepository>();
        return services;
    }
}
