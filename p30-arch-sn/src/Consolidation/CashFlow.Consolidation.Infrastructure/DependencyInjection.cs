using CashFlow.BuildingBlocks.Persistence.Interfaces;
using CashFlow.Consolidation.Domain.Repositories;
using CashFlow.Consolidation.Infrastructure.Persistence;
using CashFlow.Consolidation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Consolidation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ResolveSqliteConnectionString(
            configuration.GetConnectionString("ConsolidationDatabase"),
            "consolidation.db");

        services.AddDbContext<ConsolidationDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IDailyConsolidationRepository, DailyConsolidationRepository>();
        services.AddScoped<IProcessedLaunchEventRepository, ProcessedLaunchEventRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ConsolidationDbContext>());

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