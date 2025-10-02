using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCash.Report.Query;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationReportQuery(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}

