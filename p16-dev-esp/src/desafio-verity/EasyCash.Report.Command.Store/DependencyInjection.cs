using EasyCash.Abstractions.Interfaces;
using EasyCash.Report.Command.Store.Contexts;
using EasyCash.Report.Command.Store.Repositories.Consolidations;
using EasyCash.Report.Domain.Consolidations.Interfaces;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCash.Report.Command.Store;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureReportCommandStore(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database") ??
                                  throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<ReportDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseExceptionProcessor()
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IConsolidationTransactionCommandStore, ConsolidationTransactionCommandStore>();
        services.AddScoped<IDailyConsolidationCommandStore, DailyConsolidationCommandStore>();

        return services;
    }
}