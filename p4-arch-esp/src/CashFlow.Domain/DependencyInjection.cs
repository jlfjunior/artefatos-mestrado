using CashFlow.Domain.Account;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomainService(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();

            return services;
        }
    }
}
