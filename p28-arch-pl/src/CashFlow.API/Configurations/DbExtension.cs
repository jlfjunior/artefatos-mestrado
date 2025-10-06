using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;

namespace CashFlow.API.Configurations;

[ExcludeFromCodeCoverage]
public static class DbExtension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB");
        var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName");

        // Criando a conexão com o MongoDB
        var mongoClient = new MongoClient(connectionString);
        services.AddSingleton(mongoClient.GetDatabase(databaseName));

        return services;
    }
}