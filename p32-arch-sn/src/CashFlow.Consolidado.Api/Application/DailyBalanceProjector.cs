using CashFlow.Shared;

namespace CashFlow.Consolidado.Api.Application;

public sealed class DailyBalanceProjector
{
    public DailyBalanceSnapshot Apply(
        DailyBalanceSnapshot? current,
        EntryRegisteredIntegrationEvent integrationEvent,
        long sequenceId,
        DateTimeOffset processedAt)
    {
        var totalCredits = current?.TotalCredits ?? 0m;
        var totalDebits = current?.TotalDebits ?? 0m;

        if (integrationEvent.Type == EntryType.Credito)
        {
            totalCredits += integrationEvent.Amount;
        }
        else
        {
            totalDebits += integrationEvent.Amount;
        }

        return new DailyBalanceSnapshot(
            integrationEvent.MerchantId,
            integrationEvent.BusinessDate,
            totalCredits,
            totalDebits,
            totalCredits - totalDebits,
            integrationEvent.EventId,
            sequenceId,
            processedAt);
    }

    public DailyBalanceSnapshot Rebuild(
        string merchantId,
        DateOnly businessDate,
        IReadOnlyCollection<IntegrationEnvelope> integrationEvents,
        DateTimeOffset processedAt)
    {
        DailyBalanceSnapshot? snapshot = null;

        foreach (var integrationEnvelope in integrationEvents.OrderBy(item => item.SequenceId))
        {
            snapshot = Apply(snapshot, integrationEnvelope.Payload, integrationEnvelope.SequenceId, processedAt);
        }

        return snapshot ?? new DailyBalanceSnapshot(
            merchantId,
            businessDate,
            0m,
            0m,
            0m,
            null,
            0L,
            processedAt);
    }
}
