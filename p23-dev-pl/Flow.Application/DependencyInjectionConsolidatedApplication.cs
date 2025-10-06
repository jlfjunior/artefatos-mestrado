using Flow.Application.MessageBrokers;
using Flow.Application.UseCases.Consolidated;
using Flow.Application.UseCases.Login;
using Flow.Application.UseCases.Transaction;
using Flow.Core.Security.Tokens;
using Flow.Infra.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Application
{
    public static class DependencyInjectionConsolidatedApplication
    {
        public static void AddDependencyInjectionConsolidatedApplication(this IServiceCollection services, IConfiguration configuration)
        {
            AddUseCases(services);
        }

        public static void AddUseCases(IServiceCollection services)
        {
            services.AddScoped<IGetDailyBalanceUseCase, GetDailyBalanceUseCase>();
        }
    }
}
