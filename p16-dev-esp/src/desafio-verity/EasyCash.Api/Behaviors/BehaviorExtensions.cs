namespace EasyCash.Api.Behaviors;

public static class BehaviorExtensions
{
    public static IServiceCollection AddApplicationGenericBehaviors(this IServiceCollection services,
        MediatRServiceConfiguration configuration)
    {
        configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));

        configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));

        configuration.AddOpenBehavior(typeof(TransactionalBehavior<,>));

        configuration.AddOpenBehavior(typeof(IdempotencyBehavior<,>));

        return services;
    }
}
