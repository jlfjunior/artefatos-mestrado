using MediatR;
using Transaction.Application.UseCases.Transaction.Create;

namespace Transaction.Api.Configurations;

public static class UseCaseConfiguration
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddMediatR(typeof(Program).Assembly, typeof(CreateMovement).Assembly);
        return services;
    }
}
