using ControleLancamentos.Application.Lancamentos.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ControleLancamentos.Application;

public static class InjecaoDependencias
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICriarLancamentoService, CriarLancamentoService>();
        services.AddScoped<ICalcularConsolidadoDiarioService, CalcularConsolidadoDiarioService>();

        return services;
    }
}
