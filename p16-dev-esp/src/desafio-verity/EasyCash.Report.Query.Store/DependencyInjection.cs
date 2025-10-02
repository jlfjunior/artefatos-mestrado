using EasyCash.Dapper.Provider;
using EasyCash.Report.Query.Consolidations.Interfaces;
using EasyCash.Report.Query.Store.Consolidations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCash.Report.Query.Store;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureReportQueryStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureDapperProvider(configuration);

        services.AddScoped<IReportConsolidationQueryStore, ReportConsolidationQueryStore>();

        return services;
    }
}