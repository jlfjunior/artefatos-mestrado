using EasyCash.Abstractions.Repository;
using EasyCash.Report.Domain.Consolidations.Entity;

namespace EasyCash.Report.Domain.Consolidations.Interfaces;

public interface IDailyConsolidationCommandStore : IAddRepository<DailyConsolidationEntity, Guid>
{
    Task<(bool, Guid)> IsAlreadyConsolidatedDayAsync(DateOnly date, CancellationToken cancellationToken);
}