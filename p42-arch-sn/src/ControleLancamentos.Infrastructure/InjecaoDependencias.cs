using ControleLancamentos.Application.Lancamentos.Eventos;
using ControleLancamentos.Application.Lancamentos.Repositorios;
using ControleLancamentos.Infrastructure.Eventos;
using ControleLancamentos.Infrastructure.Persistencia;
using ControleLancamentos.Infrastructure.Repositorios;
using ControleLancamentos.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControleLancamentos.Infrastructure;

public static class InjecaoDependencias
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("ControleLancamentos")
            ?? throw new InvalidOperationException("A connection string 'ControleLancamentos' não foi configurada.");

        services.AddDbContext<ControleLancamentosDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ILancamentoRepositorio, LancamentoRepositorio>();
        services.AddScoped<IConsolidadoDiarioRepositorio, ConsolidadoDiarioRepositorio>();

        services.AddSingleton<InMemoryLancamentoCriadoEventBus>();
        services.AddSingleton<ILancamentoCriadoEventBus>(provider =>
            provider.GetRequiredService<InMemoryLancamentoCriadoEventBus>());
        services.AddHostedService<AtualizarConsolidadoDiarioWorker>();

        return services;
    }
}
