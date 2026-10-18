using Shared.DTOs;

namespace Transactions.API.src.Application.Interface
{
    public interface ITransactionService
    {
        Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
        Task<TransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PaginationResponse<TransactionDto>> GetByDateRangeAsync(
            DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}
