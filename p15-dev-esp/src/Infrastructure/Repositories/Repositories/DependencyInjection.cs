using Domain.Entities.Consolidation;
using Domain.Entities.Launch;
using Domain.Entities.LaunchProduct;
using Domain.Entities.Product;
using Infrastructure.Repositories.Repositories.Consolidation;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Repositories.Repositories.Generic.GenericMongoDB;

namespace Infrastructure.Repositories.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddLaunchRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IMongoDBRepository<>), typeof(MongoDBRepository<>));

        services.AddScoped<ILaunchRepository, LaunchRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ILaunchProductRepository, LaunchProductRepository>();

        services.AddScoped<IConsolidationRepository>(provider => {
            var database = provider.GetRequiredService<IMongoDatabase>();
            return new ConsolidationRepository(database, "Consolidations");
        });

        return services;
    }

    public static IServiceCollection AddConsolidationRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IMongoDBRepository<>), typeof(MongoDBRepository<>));

        services.AddScoped<IConsolidationRepository>(provider => {
            var database = provider.GetRequiredService<IMongoDatabase>();
            return new ConsolidationRepository(database, "Consolidations");
        });

        return services;
    }
}