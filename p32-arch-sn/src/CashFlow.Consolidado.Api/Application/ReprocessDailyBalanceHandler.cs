using CashFlow.Consolidado.Api.Infrastructure;
using CashFlow.Shared;

namespace CashFlow.Consolidado.Api.Application;

public sealed class ReprocessDailyBalanceHandler
{
    private readonly IIntegrationEventFeed _integrationEventFeed;
    private readonly IDailyBalanceProjectionStore _projectionStore;
    private readonly DailyBalanceProjector _projector;
    private readonly TimeProvider _timeProvider;

    public ReprocessDailyBalanceHandler(
        IIntegrationEventFeed integrationEventFeed,
        IDailyBalanceProjectionStore projectionStore,
        DailyBalanceProjector projector,
        TimeProvider timeProvider)
    {
        _integrationEventFeed = integrationEventFeed;
        _projectionStore = projectionStore;
        _projector = projector;
        _timeProvider = timeProvider;
    }

    public async Task<DailyBalanceResponse> HandleAsync(
        ReprocessDailyBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var allEvents = await _integrationEventFeed.ReadAllAsync(cancellationToken);
        var filteredEvents = allEvents
            .Where(item => string.Equals(item.Payload.MerchantId, request.MerchantId, StringComparison.OrdinalIgnoreCase))
            .Where(item => item.Payload.BusinessDate == request.BusinessDate)
            .ToArray();

        var rebuilt = _projector.Rebuild(
            request.MerchantId,
            request.BusinessDate,
            filteredEvents,
            _timeProvider.GetUtcNow());

        await _projectionStore.ReplaceAsync(rebuilt, cancellationToken);

        return new DailyBalanceResponse(
            rebuilt.MerchantId,
            rebuilt.BusinessDate,
            rebuilt.TotalCredits,
            rebuilt.TotalDebits,
            rebuilt.Balance,
            rebuilt.UpdatedAt,
            "REPROCESSADO");
    }
}

