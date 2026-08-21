using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Consolidado.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InicializarBancoConsolidadoAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ConsolidadoDbContext>();
        await db.Database.EnsureCreatedAsync(ct);
    }
}
