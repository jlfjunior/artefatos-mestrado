using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlowControl.Core.Infrastructure.Configurations.ResolveDI
{
    public static class ResolveRepositoriesDI
    {
        public static void RegistryRepositories(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IConsolidatedBalanceRepository, ConsolidatedBalanceRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
        }
    }
}
