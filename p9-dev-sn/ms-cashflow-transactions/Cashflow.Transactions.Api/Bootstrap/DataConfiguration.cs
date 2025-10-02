using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Data.Configs;
using Cashflow.Transactions.Data.QueryServices;

namespace Cashflow.Transactions.Api.Bootstrap
{
    public static class DataConfiguration
    {
        public static IServiceCollection ConfigureData(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CashflowDbConfig>(configuration.GetSection("CashflowDatabase"));
            services.AddTransient<ITransactionsQueryService, TransactionsQueryService>();

            return services;
        }
    }
}
