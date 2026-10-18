using Consolidado.Application.Ports;
using Consolidado.Domain;
using Microsoft.EntityFrameworkCore;

namespace Consolidado.Infrastructure.Persistence;

public class SaldoDiarioRepository : ISaldoDiarioRepository
{
    private readonly ConsolidadoDbContext _db;

    public SaldoDiarioRepository(ConsolidadoDbContext db) => _db = db;

    public Task<SaldoDiario?> ObterPorDataAsync(DateOnly data, CancellationToken ct = default)
        => _db.SaldosDiarios.FirstOrDefaultAsync(s => s.Data == data, ct);

    public async Task<IReadOnlyList<SaldoDiario>> ListarPorPeriodoAsync(DateOnly de, DateOnly ate, CancellationToken ct = default)
        => await _db.SaldosDiarios.AsNoTracking()
            .Where(s => s.Data >= de && s.Data <= ate)
            .OrderBy(s => s.Data)
            .ToListAsync(ct);

    public async Task SalvarAsync(SaldoDiario saldo, CancellationToken ct = default)
    {
        var existe = await _db.SaldosDiarios.AnyAsync(s => s.Data == saldo.Data, ct);
        if (existe)
            _db.SaldosDiarios.Update(saldo);
        else
            await _db.SaldosDiarios.AddAsync(saldo, ct);
        // Commit fica a cargo do UnitOfWork.
    }
}
