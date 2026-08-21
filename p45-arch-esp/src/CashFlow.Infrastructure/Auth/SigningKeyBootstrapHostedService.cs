using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Garante o pool configurado de chaves RSA na inicialização da API.
/// </summary>
/// <param name="serviceProvider">Provedor de serviços para resolver dependências scoped.</param>
/// <param name="logger">Logger da inicialização.</param>
public sealed class SigningKeyBootstrapHostedService(
    IServiceProvider serviceProvider,
    ILogger<SigningKeyBootstrapHostedService> logger) : IHostedService
{
    /// <summary>
    /// Inicializa o pool de chaves de assinatura na subida do host.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task concluída após a inicialização.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var signingKeys = scope.ServiceProvider.GetRequiredService<SigningKeysService>();
        await signingKeys.EnsurePoolAsync(cancellationToken);
        logger.LogInformation("Signing key pool initialized.");
    }

    /// <summary>
    /// Encerramento do serviço hospedado (sem ação necessária).
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task concluída imediatamente.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
