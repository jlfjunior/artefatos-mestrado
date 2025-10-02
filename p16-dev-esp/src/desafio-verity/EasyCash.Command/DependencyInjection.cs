using EasyCash.Abstractions.Interfaces;
using EasyCash.Domain;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCash.Command;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCommand(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();


        return services;
    }
}

