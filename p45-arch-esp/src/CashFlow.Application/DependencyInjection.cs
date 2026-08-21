using CashFlow.Application.Authorization;
using CashFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Application;

/// <summary>
/// Extensões de registro da camada de aplicação.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra serviços de casos de uso.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <returns>Coleção de serviços.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddScoped<TransactionService>()
            .AddScoped<BalanceService>()
            .AddScoped<ClientService>()
            .AddScoped<DailyBalanceProjector>()
            .AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, AnyScopeAuthorizationHandler>();
    }
}
