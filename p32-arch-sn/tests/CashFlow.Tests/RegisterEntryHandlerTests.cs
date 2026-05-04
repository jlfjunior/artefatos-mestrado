using CashFlow.Lancamentos.Api.Application;
using CashFlow.Shared;

namespace CashFlow.Tests;

public sealed class RegisterEntryHandlerTests
{
    [Fact]
    public async Task Deve_criar_lancamento_e_enfileirar_evento_na_saida_transacional()
    {
        var store = new InMemoryEntryCommandStore();
        var handler = new RegisterEntryHandler(
            store,
            new FrozenTimeProvider(new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero)));

        var request = new RegisterEntryRequest(
            "lojista-01",
            new DateOnly(2026, 4, 16),
            EntryType.Credito,
            125.50m,
            "Venda no PDV",
            "PDV");

        var result = await handler.HandleAsync(request, "idem-001", CancellationToken.None);

        Assert.Equal(RegisterEntryStatus.Criado, result.Status);
        Assert.NotNull(result.Response);
        Assert.Single(store.SavedEvents);
        Assert.Equal(125.50m, result.Response!.Amount);
    }

    [Fact]
    public async Task Deve_retornar_reenvio_quando_o_mesmo_payload_usar_a_mesma_chave_idempotente()
    {
        var store = new InMemoryEntryCommandStore();
        var handler = new RegisterEntryHandler(
            store,
            new FrozenTimeProvider(new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero)));

        var request = new RegisterEntryRequest(
            "lojista-01",
            new DateOnly(2026, 4, 16),
            EntryType.Credito,
            80m,
            "Recebimento",
            "API");

        await handler.HandleAsync(request, "idem-002", CancellationToken.None);
        var replay = await handler.HandleAsync(request, "idem-002", CancellationToken.None);

        Assert.Equal(RegisterEntryStatus.ReenvioIdempotente, replay.Status);
        Assert.Single(store.SavedEvents);
        Assert.Equal("REENVIADO", replay.Response!.Status);
    }

    [Fact]
    public async Task Deve_rejeitar_quando_a_mesma_chave_idempotente_receber_payload_diferente()
    {
        var store = new InMemoryEntryCommandStore();
        var handler = new RegisterEntryHandler(
            store,
            new FrozenTimeProvider(new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero)));

        var original = new RegisterEntryRequest(
            "lojista-01",
            new DateOnly(2026, 4, 16),
            EntryType.Credito,
            100m,
            "Venda",
            "PDV");

        var changed = original with { Amount = 150m };

        await handler.HandleAsync(original, "idem-003", CancellationToken.None);
        var conflict = await handler.HandleAsync(changed, "idem-003", CancellationToken.None);

        Assert.Equal(RegisterEntryStatus.Conflito, conflict.Status);
        Assert.Null(conflict.Response);
        Assert.Single(store.SavedEvents);
    }
}
