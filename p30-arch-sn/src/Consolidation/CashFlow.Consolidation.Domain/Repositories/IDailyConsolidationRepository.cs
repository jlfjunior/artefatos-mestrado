using CashFlow.Consolidation.Domain.Entities;

namespace CashFlow.Consolidation.Domain.Repositories;

public interface IDailyConsolidationRepository
{
    Task<DailyConsolidation?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<DailyConsolidation?> GetByDateForUpdateAsync(DateOnly date, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyCollection<DailyConsolidation>> GetByPeriodAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(DailyConsolidation consolidation, CancellationToken cancellationToken = default);
}