using CashFlow.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CashFlow.Infrastructure.Health;

/// <summary>
/// Verifica conectividade com PostgreSQL.
/// </summary>
/// <param name="connectionFactory">Factory de conexões PostgreSQL.</param>
public sealed class PostgresHealthCheck(DbConnectionFactory connectionFactory) : IHealthCheck
{
    /// <summary>
    /// Executa SELECT 1 para validar conectividade com o banco.
    /// </summary>
    /// <param name="context">Contexto da verificação de saúde.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado healthy ou unhealthy conforme a conexão.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await connectionFactory.OpenNewConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL connection is OK.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed.", ex);
        }
    }
}
