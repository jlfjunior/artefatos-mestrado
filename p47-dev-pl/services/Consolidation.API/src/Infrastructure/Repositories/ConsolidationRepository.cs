using Consolidation.API.Domain.Interfaces;
using Consolidation.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Consolidation.API.Infrastructure.Repositories;

public class ConsolidationRepository : IConsolidationRepository
{
    private readonly ConsolidationDbContext _context;
    private readonly ILogger<ConsolidationRepository> _logger;

    public ConsolidationRepository(ConsolidationDbContext context, ILogger<ConsolidationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Domain.Models.Consolidation> AddAsync(Domain.Models.Consolidation consolidation, CancellationToken cancellationToken = default)
    {
        if (consolidation == null)
            throw new ArgumentNullException(nameof(consolidation));

        _context.Consolidations.Add(consolidation);
        _logger.LogDebug("Consolidação adicionada: {ConsolidationId}", consolidation.Id);
        return consolidation;
    }

    public async Task<Domain.Models.Consolidation?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        return await _context.Consolidations
            .FirstOrDefaultAsync(c => c.ConsolidationDate == dateOnly, cancellationToken);
    }

    public async Task<Domain.Models.Consolidation?> GetByLojistaAsync(Lojista lojista, CancellationToken cancellationToken = default)
    {        
        return await _context.Consolidations
            .FirstOrDefaultAsync(c => c.Lojista == lojista, cancellationToken);
    }

    public async Task<List<Domain.Models.Consolidation>> GetByDateRangeAsync(
        DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1);

        return await _context.Consolidations
            .Where(c => c.ConsolidationDate >= start && c.ConsolidationDate < end)
            .OrderBy(c => c.Lojista)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Consolidations.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(c => c.ConsolidationDate >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(c => c.ConsolidationDate <= endDate.Value.Date);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Consolidation.API.Domain.Models.Consolidation> UpdateAsync(Domain.Models.Consolidation consolidation, CancellationToken cancellationToken = default)
    {
        _context.Consolidations.Update(consolidation);
        return consolidation;
    }
}
