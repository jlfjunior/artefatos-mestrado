using CashFlowControl.Core.Application.Commands;
using CashFlowControl.Core.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace CashFlowControl.Core.Application.ResolveDI
{
    public static class SecurityModuleStartupDI
    {
        public static IServiceCollection ConfigureSecurityModule(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ValidateTokenCommand).Assembly))
                .AddValidatorsFromAssemblyContaining<ValidateTokenForBearerSchemaCommandValidator>();
        }
    }
}
