using Consolidation.Application.UseCases.Consolidation.Get;
using MediatR;

namespace Consolidation.Api.Configurations;

public static class UseCaseConfiguration
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddMediatR(typeof(Program).Assembly, typeof(GetConsolidate).Assembly);
        return services;
    }
}
