using Consolidado.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Consolidado.Infrastructure.Persistence;

public class LancamentosProcessadosStore : ILancamentosProcessadosStore
{
    private readonly ConsolidadoDbContext _db;

    public LancamentosProcessadosStore(ConsolidadoDbContext db) => _db = db;

    public Task<bool> JaProcessadoAsync(Guid lancamentoId, CancellationToken ct = default)
        => _db.LancamentosProcessados.AnyAsync(l => l.LancamentoId == lancamentoId, ct);

    public async Task MarcarComoProcessadoAsync(Guid lancamentoId, CancellationToken ct = default)
    {
        await _db.LancamentosProcessados.AddAsync(
            new LancamentoProcessado { LancamentoId = lancamentoId, ProcessadoEmUtc = DateTime.UtcNow }, ct);
    }
}
