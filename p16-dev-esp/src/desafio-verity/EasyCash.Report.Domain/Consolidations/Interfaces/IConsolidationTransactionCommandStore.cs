using EasyCash.Abstractions.Repository;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Report.Domain.Consolidations.Models;

namespace EasyCash.Report.Domain.Consolidations.Interfaces;

public interface IConsolidationTransactionCommandStore : IAddRepository<ConsolidationTransactionEntity, Guid>
{
    Task<DailyConsolidationModel> GetDailyConsolidationAsync(DateOnly date, CancellationToken cancellationToken);
}