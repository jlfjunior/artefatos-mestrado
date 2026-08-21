using CashFlow.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CashFlow.Infrastructure.Health;

/// <summary>
/// Verifica conectividade com RabbitMQ.
/// </summary>
/// <param name="connectionHolder">Gerenciador de conexão com o broker.</param>
public sealed class RabbitMqHealthCheck(RabbitMqConnectionHolder connectionHolder) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await connectionHolder.GetConnectionAsync(cancellationToken);
            if (!connection.IsOpen)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ connection is closed.");
            }

            using var channel = connection.CreateModel();
            return HealthCheckResult.Healthy("RabbitMQ connection is OK.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection failed.", ex);
        }
    }
}
