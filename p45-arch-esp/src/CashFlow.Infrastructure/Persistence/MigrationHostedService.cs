using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CashFlow.Infrastructure.Persistence;

/// <summary>
/// Executa migrations FluentMigrator na inicialização do host.
/// </summary>
/// <param name="serviceProvider">Provedor de serviços para resolver o runner de migrations.</param>
public sealed class MigrationHostedService(IServiceProvider serviceProvider) : IHostedService
{
    /// <summary>
    /// Aplica migrations pendentes na subida do host.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task concluída após MigrateUp.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Encerramento do serviço hospedado (sem ação necessária).
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task concluída imediatamente.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
