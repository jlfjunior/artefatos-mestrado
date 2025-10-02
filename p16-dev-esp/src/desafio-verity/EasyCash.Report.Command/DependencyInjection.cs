using EasyCash.Abstractions.Interfaces;
using EasyCash.Report.Domain;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCash.Report.Command;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationReportCommand(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();


        return services;
    }
}

