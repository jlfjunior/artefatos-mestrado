using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Repositories;
using CashFlow.Consolidation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Repositories;

public sealed class DailyConsolidationRepository(ConsolidationDbContext dbContext) : IDailyConsolidationRepository
{
    public async Task<DailyConsolidation?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.DailyConsolidations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Date == date, cancellationToken);
    }
    
    public async Task<DailyConsolidation?> GetByDateForUpdateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.DailyConsolidations
            .FirstOrDefaultAsync(x => x.Date == date, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DailyConsolidation>> GetByPeriodAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.DailyConsolidations
            .AsNoTracking()
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DailyConsolidation consolidation, CancellationToken cancellationToken = default)
    {
        await dbContext.DailyConsolidations.AddAsync(consolidation, cancellationToken);
    }
}