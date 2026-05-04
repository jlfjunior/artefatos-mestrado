using CashFlow.Consolidation.Domain.Entities;

namespace CashFlow.Consolidation.Domain.Repositories;

public interface IProcessedLaunchEventRepository
{
    Task<bool> ExistsByLaunchIdAsync(Guid launchId, CancellationToken cancellationToken = default);

    Task AddAsync(ProcessedLaunchEvent processedLaunchEvent, CancellationToken cancellationToken = default);
}
