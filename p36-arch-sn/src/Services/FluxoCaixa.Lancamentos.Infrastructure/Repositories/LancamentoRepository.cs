using FluxoCaixa.Lancamentos.Domain.Entities;
using FluxoCaixa.Lancamentos.Domain.Repositories;
using FluxoCaixa.Lancamentos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FluxoCaixa.Lancamentos.Infrastructure.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly LancamentosDbContext _context;

    public LancamentoRepository(LancamentosDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
    {
        await _context.Lancamentos.AddAsync(lancamento, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lancamento>> GetByDataAsync(DateOnly data, string fusoHorario, CancellationToken cancellationToken = default)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(fusoHorario);
        
        // Data inicial no fuso escolhido -> convertido para UTC
        var startDateTimeLocal = data.ToDateTime(TimeOnly.MinValue);
        var startOffset = new DateTimeOffset(startDateTimeLocal, tz.GetUtcOffset(startDateTimeLocal));
        var utcStart = startOffset.ToUniversalTime();

        var endDateTimeLocal = data.ToDateTime(TimeOnly.MaxValue);
        var endOffset = new DateTimeOffset(endDateTimeLocal, tz.GetUtcOffset(endDateTimeLocal));
        var utcEnd = endOffset.ToUniversalTime();

        return await _context.Lancamentos
            .Where(x => x.DataHora >= utcStart && x.DataHora <= utcEnd)
            .OrderByDescending(x => x.DataHora)
            .ToListAsync(cancellationToken);
    }
}
