using CashFlow.Consolidado.Api.Application;
using CashFlow.Shared;

namespace CashFlow.Tests;

public sealed class ReprocessDailyBalanceHandlerTests
{
    [Fact]
    public async Task Deve_reprocessar_uma_data_de_negocio_especifica()
    {
        var feed = new InMemoryIntegrationEventFeed();
        var store = new InMemoryProjectionStore();
        var timeProvider = new FrozenTimeProvider(new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero));

        feed.Events.Add(new IntegrationEnvelope(
            1,
            new EntryRegisteredIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "lojista-01", new DateOnly(2026, 4, 16), EntryType.Credito, 120m, "Venda", "PDV", timeProvider.GetUtcNow())));
        feed.Events.Add(new IntegrationEnvelope(
            2,
            new EntryRegisteredIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "lojista-01", new DateOnly(2026, 4, 16), EntryType.Debito, 30m, "Taxa", "ERP", timeProvider.GetUtcNow())));
        feed.Events.Add(new IntegrationEnvelope(
            3,
            new EntryRegisteredIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "lojista-02", new DateOnly(2026, 4, 16), EntryType.Credito, 999m, "Outra Loja", "API", timeProvider.GetUtcNow())));

        var handler = new ReprocessDailyBalanceHandler(
            feed,
            store,
            new DailyBalanceProjector(),
            timeProvider);

        var response = await handler.HandleAsync(
            new ReprocessDailyBalanceRequest("lojista-01", new DateOnly(2026, 4, 16)),
            CancellationToken.None);

        Assert.Equal(120m, response.TotalCredits);
        Assert.Equal(30m, response.TotalDebits);
        Assert.Equal(90m, response.Balance);
        Assert.Equal("REPROCESSADO", response.ConsistencyStatus);
        Assert.NotNull(store.Snapshot);
    }
}
