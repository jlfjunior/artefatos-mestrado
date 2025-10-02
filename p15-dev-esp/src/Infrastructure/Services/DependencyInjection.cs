using Application.Authentication.Services;
using Infrastructure.Repositories.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;

namespace Infrastructure.Services.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddLaunchServices(this IServiceCollection services)
    {
        services.AddScoped<ILaunchService, LaunchService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddConsolidationServices(this IServiceCollection services)
    {
        services.AddScoped<IConsolidationService, ConsolidationService>();

        return services;
    }
}