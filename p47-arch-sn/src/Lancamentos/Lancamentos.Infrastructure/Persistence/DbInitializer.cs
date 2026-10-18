using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lancamentos.Infrastructure.Persistence;

public static class DbInitializer
{
    /// <summary>
    /// Cria o schema (tabela de lançamentos + tabelas de outbox do MassTransit)
    /// caso ainda não exista. Optei por EnsureCreated em vez de migrations para
    /// manter o setup do desafio simples — num projeto de produção isso seria
    /// substituído por migrations versionadas.
    /// </summary>
    public static async Task InicializarBancoLancamentosAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();
        await db.Database.EnsureCreatedAsync(ct);
    }
}
