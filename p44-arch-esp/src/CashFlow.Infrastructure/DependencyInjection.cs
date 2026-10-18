using CashFlow.Application.Abstractions;
using CashFlow.Infrastructure.Auth;
using CashFlow.Infrastructure.Configuration;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence;
using CashFlow.Infrastructure.Persistence.Repositories;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

/// <summary>
/// Extensões de registro da camada de infraestrutura.
/// </summary>
public static class DependencyInjection
{
    static DependencyInjection()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
    }

    /// <summary>
    /// Registra persistência, mensageria e migrations (host com escrita).
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração do aplicativo.</param>
    /// <returns>Coleção de serviços.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.RegisterPersistence(configuration)
            .RegisterRabbitMq(configuration)
            .RegisterMigrations(configuration)
            .AddScoped<ITransactionRepository, TransactionRepository>()
            .AddScoped<IDailyBalanceRepository, DailyBalanceRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IOAuthClientRepository, OAuthClientRepository>()
            .AddScoped<ISigningKeyRepository, SigningKeyRepository>()
            .AddScoped<IUnitOfWork, PostgresUnitOfWork>()
            .AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
    }

    /// <summary>
    /// Registra persistência e migrations para consumer (sem publicador).
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração do aplicativo.</param>
    /// <returns>Coleção de serviços.</returns>
    public static IServiceCollection AddConsumerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.RegisterPersistence(configuration)
            .RegisterRabbitMq(configuration)
            .RegisterMigrations(configuration)
            .AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
    }

    private static IServiceCollection RegisterPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName))
            .AddSingleton<DbConnectionFactory>()
            .AddScoped<ScopedDbConnection>();
    }

    private static IServiceCollection RegisterRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName))
            .AddSingleton<RabbitMqConnectionHolder>();
    }

    /// <summary>
    /// Registra migrations FluentMigrator.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração do aplicativo.</param>
    /// <returns>Coleção de serviços.</returns>
    private static IServiceCollection RegisterMigrations(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()?.ConnectionString
            ?? throw new InvalidOperationException("Database connection string is required.");

        return services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(DependencyInjection).Assembly).For.Migrations())
            .AddHostedService<MigrationHostedService>();
    }
}
