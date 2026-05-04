using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Repositories;
using CashFlow.Consolidation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Repositories;

public sealed class ProcessedLaunchEventRepository(ConsolidationDbContext dbContext) : IProcessedLaunchEventRepository
{
    public Task<bool> ExistsByLaunchIdAsync(Guid launchId, CancellationToken cancellationToken = default)
    {
        return dbContext.ProcessedLaunchEvents
            .AsNoTracking()
            .AnyAsync(x => x.LaunchId == launchId, cancellationToken);
    }

    public async Task AddAsync(
        ProcessedLaunchEvent processedLaunchEvent,
        CancellationToken cancellationToken = default)
    {
        await dbContext.ProcessedLaunchEvents.AddAsync(processedLaunchEvent, cancellationToken);
    }
}
