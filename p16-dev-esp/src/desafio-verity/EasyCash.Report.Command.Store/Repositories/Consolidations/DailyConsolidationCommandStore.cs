using EasyCash.Report.Command.Store.Contexts;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Report.Domain.Consolidations.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EasyCash.Report.Command.Store.Repositories.Consolidations;

internal sealed class DailyConsolidationCommandStore : Repository<DailyConsolidationEntity, Guid>, IDailyConsolidationCommandStore
{
    public DailyConsolidationCommandStore(ReportDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(bool, Guid)> IsAlreadyConsolidatedDayAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var entity = await DbContext
            .Set<DailyConsolidationEntity>()
            .AsNoTracking()
            .Where(e => DateOnly.FromDateTime(e.Date.Date) == date)
            .Select(e => new { e.Id })
            .FirstOrDefaultAsync(cancellationToken);

        return entity != null ?
            (true, entity.Id) :
            (false, Guid.Empty);
    }
}