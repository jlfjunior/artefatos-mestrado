using CashFlowControl.Core.Application.Commands;
using CashFlowControl.Core.Application.Commands.Auth;
using CashFlowControl.Core.Application.Handlers;
using CashFlowControl.Core.Application.Queries.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlowControl.Core.Application.ResolveDI
{
    public static class ResolveAuthHandlers
    {
        public static IServiceCollection ConfigureAuthCQRS(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthGetRefreshTokenQuery).Assembly))
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthAuthenticateCommand).Assembly))
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthAuthenticateCommand).Assembly))
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthSaveRefreshTokenCommand).Assembly));
        }
    }
}
