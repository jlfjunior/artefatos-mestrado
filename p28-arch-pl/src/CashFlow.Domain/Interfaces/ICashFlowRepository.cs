using CashFlow.Domain.Aggregates;

namespace CashFlow.Domain.Interfaces;

public interface ICashFlowRepository
{
    Task<CashFlowDailyAggregate?> GetCurrentCashByAccountId(Guid accountId);

    Task<(List<CashFlowDailyAggregate?> response, long totalItemCount, int totalPages)> GetByAccountIdAndDateRange(
        Guid accountId, DateOnly startDate, DateOnly endDate, int pageNumber = 1, int pageSize = 50);

    Task<CashFlowDailyAggregate?> GetByTransactionId(Guid transactionId);
    Task Save(CashFlowDailyAggregate cashFlowDaily);
}