using FluxoCaixa.Consolidado.Domain.Entities;
using FluxoCaixa.Consolidado.Domain.Repositories;
using FluxoCaixa.Consolidado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FluxoCaixa.Consolidado.Infrastructure.Repositories;

public class ConsolidadoRepository : IConsolidadoRepository
{
    private readonly ConsolidadoDbContext _context;

    public ConsolidadoRepository(ConsolidadoDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ConsolidadoDiario consolidado, CancellationToken cancellationToken = default)
    {
        await _context.Consolidados.AddAsync(consolidado, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConsolidadoDiario?> GetByDataAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        return await _context.Consolidados.FirstOrDefaultAsync(x => x.Data == data, cancellationToken);
    }

    public async Task<IEnumerable<ConsolidadoDiario>> GetRangeAsync(DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default)
    {
        return await _context.Consolidados
            .Where(x => x.Data >= inicio && x.Data <= fim)
            .OrderBy(x => x.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasEventBeenProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedEvents.AnyAsync(x => x.EventId == eventId, cancellationToken);
    }

    public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _context.ProcessedEvents.AddAsync(new ProcessedEvent(eventId), cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ConsolidadoDiario consolidado, CancellationToken cancellationToken = default)
    {
        _context.Consolidados.Update(consolidado);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AplicarLancamentoAtomicAsync(DateOnly data, string tipo, decimal valor, CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var credito = tipo.ToUpper() == "CREDITO" ? valor : 0m;
        var debito  = tipo.ToUpper() == "CREDITO" ? 0m : valor;

        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            MERGE Consolidados WITH (HOLDLOCK) AS target
            USING (VALUES ({data})) AS source (Data)
                ON target.Data = source.Data
            WHEN MATCHED THEN
                UPDATE SET
                    TotalCreditos          = target.TotalCreditos + {credito},
                    TotalDebitos           = target.TotalDebitos  + {debito},
                    QuantidadeLancamentos  = target.QuantidadeLancamentos + 1,
                    UltimaAtualizacao      = {agora}
            WHEN NOT MATCHED THEN
                INSERT (Data, TotalCreditos, TotalDebitos, QuantidadeLancamentos, UltimaAtualizacao)
                VALUES ({data}, {credito}, {debito}, 1, {agora});
            """, cancellationToken);
    }
}
