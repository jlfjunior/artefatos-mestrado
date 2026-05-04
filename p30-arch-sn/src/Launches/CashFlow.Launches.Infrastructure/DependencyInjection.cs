using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.Launches.Application.Interfaces;
using CashFlow.Launches.Domain.Repositories;
using CashFlow.Launches.Infrastructure.Messaging;
using CashFlow.Launches.Infrastructure.Persistence;
using CashFlow.Launches.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Launches.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ResolveSqliteConnectionString(
            configuration.GetConnectionString("LaunchesDatabase"),
            "launches.db");

        services.AddDbContext<LaunchesDbContext>(options => options.UseSqlite(connectionString));
        services.Configure<RabbitMqOptions>(options =>
        {
            var section = configuration.GetSection("RabbitMq");
            options.HostName = section["HostName"] ?? options.HostName;
            options.Port = int.TryParse(section["Port"], out var port) ? port : options.Port;
            options.UserName = section["UserName"] ?? options.UserName;
            options.Password = section["Password"] ?? options.Password;
            options.VirtualHost = section["VirtualHost"] ?? options.VirtualHost;
            options.ExchangeName = section["ExchangeName"] ?? options.ExchangeName;
            options.QueueName = section["QueueName"] ?? options.QueueName;
            options.RoutingKey = section["RoutingKey"] ?? options.RoutingKey;
        });

        services.AddScoped<ILaunchRepository, LaunchRepository>();
        services.AddScoped<ILaunchIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LaunchesDbContext>());

        return services;
    }

    private static string ResolveSqliteConnectionString(string? configuredConnectionString, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
            return configuredConnectionString;

        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "data");

        Directory.CreateDirectory(basePath);

        var dbPath = Path.Combine(basePath, fileName);
        return $"Data Source={dbPath}";
    }
}