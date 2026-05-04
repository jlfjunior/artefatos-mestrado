using CashFlow.Consolidado.Api.Infrastructure;
using CashFlow.Shared;

namespace CashFlow.Consolidado.Api.Application;

public sealed class GetDailyBalanceHandler
{
    private readonly IDailyBalanceProjectionStore _projectionStore;
    private readonly TimeProvider _timeProvider;

    public GetDailyBalanceHandler(IDailyBalanceProjectionStore projectionStore, TimeProvider timeProvider)
    {
        _projectionStore = projectionStore;
        _timeProvider = timeProvider;
    }

    public async Task<DailyBalanceResponse> HandleAsync(
        string merchantId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        var snapshot = await _projectionStore.GetAsync(merchantId, businessDate, cancellationToken);
        if (snapshot is null)
        {
            return new DailyBalanceResponse(
                merchantId,
                businessDate,
                0m,
                0m,
                0m,
                null,
                "SEM_MOVIMENTACAO");
        }

        return new DailyBalanceResponse(
            snapshot.MerchantId,
            snapshot.BusinessDate,
            snapshot.TotalCredits,
            snapshot.TotalDebits,
            snapshot.Balance,
            snapshot.UpdatedAt,
            snapshot.UpdatedAt >= _timeProvider.GetUtcNow().AddMinutes(-1) ? "ATUALIZADO" : "DEFASADO");
    }
}

