using EasyCash.Dapper.Provider;
using EasyCash.Query.Store.Transactions;
using EasyCash.Query.Store.Users;
using EasyCash.Query.Transactions.Interfaces;
using EasyCash.Query.Users.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCash.Query.Store;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureQueryStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureDapperProvider(configuration);

        services.AddTransient<ITransactionQueryStore, TransactionQueryStore>();

        services.AddScoped<IUserQueryStore, UserQueryStore>();

        return services;
    }
}