using ControleLancamentos.Application.Lancamentos.Eventos;
using ControleLancamentos.Application.Lancamentos.Repositorios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ControleLancamentos.Infrastructure.Workers;

public class AtualizarConsolidadoDiarioWorker(
    ILancamentoCriadoEventBus eventBus,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evento in eventBus.ConsumirAsync(stoppingToken))
        {
            using var scope = serviceScopeFactory.CreateScope();
            var repositorio = scope.ServiceProvider.GetRequiredService<IConsolidadoDiarioRepositorio>();
            await repositorio.AcumularLancamentoAsync(evento, stoppingToken);
        }
    }
}
