using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure.Health;

/// <summary>
/// Registro de health checks de infraestrutura (PostgreSQL e RabbitMQ).
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adiciona verificações de readiness para PostgreSQL e RabbitMQ.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <returns>Coleção de serviços.</returns>
    public static IServiceCollection AddCashFlowHealthChecks(this IServiceCollection services) =>
        services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgresql")
            .AddCheck<RabbitMqHealthCheck>("rabbitmq")
            .Services;
}
