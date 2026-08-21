using Shared.Enums;

namespace Consolidation.API.Domain.Interfaces;

public interface IConsolidationRepository
{
    Task<Consolidation.API.Domain.Models.Consolidation> AddAsync(Consolidation.API.Domain.Models.Consolidation consolidation, CancellationToken cancellationToken = default);
    Task<Consolidation.API.Domain.Models.Consolidation?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<Consolidation.API.Domain.Models.Consolidation?> GetByLojistaAsync(Lojista lojista, CancellationToken cancellationToken = default);
    Task<List<Consolidation.API.Domain.Models.Consolidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate,
        int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    Task<Consolidation.API.Domain.Models.Consolidation> UpdateAsync(Consolidation.API.Domain.Models.Consolidation consolidation, CancellationToken cancellationToken = default);
}
