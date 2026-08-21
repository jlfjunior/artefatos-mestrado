using Shared.Contracts;
using Shared.DTOs;

namespace Consolidation.API.src.Application.Interface
{
    public interface IConsolidationService
    {
        Task<ConsolidationDto> ProcessTransactionAsync(TransactionCreatedEvent msg, CancellationToken cancellationToken = default);
        Task<ConsolidationDto?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<PaginationResponse<ConsolidationDto>> GetByDateRangeAsync(
            DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}
