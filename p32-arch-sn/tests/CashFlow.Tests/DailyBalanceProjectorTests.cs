using CashFlow.Consolidado.Api.Application;
using CashFlow.Shared;

namespace CashFlow.Tests;

public sealed class DailyBalanceProjectorTests
{
    [Fact]
    public void Deve_acumular_credito_e_debito_no_saldo_diario()
    {
        var projector = new DailyBalanceProjector();
        var processedAt = new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero);

        var eventoCredito = new EntryRegisteredIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "lojista-01",
            new DateOnly(2026, 4, 16),
            EntryType.Credito,
            200m,
            "Venda",
            "PDV",
            processedAt);

        var eventoDebito = new EntryRegisteredIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "lojista-01",
            new DateOnly(2026, 4, 16),
            EntryType.Debito,
            35m,
            "Taxa",
            "ERP",
            processedAt);

        var aposCredito = projector.Apply(null, eventoCredito, 1, processedAt);
        var aposDebito = projector.Apply(aposCredito, eventoDebito, 2, processedAt);

        Assert.Equal(200m, aposDebito.TotalCredits);
        Assert.Equal(35m, aposDebito.TotalDebits);
        Assert.Equal(165m, aposDebito.Balance);
    }

    [Fact]
    public void Deve_reconstruir_o_retrato_a_partir_do_historico_de_integracao()
    {
        var projector = new DailyBalanceProjector();
        var processedAt = new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero);

        var eventos = new[]
        {
            new IntegrationEnvelope(
                2,
                new EntryRegisteredIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "lojista-01", new DateOnly(2026, 4, 16), EntryType.Debito, 20m, "Taxa", "ERP", processedAt)),
            new IntegrationEnvelope(
                1,
                new EntryRegisteredIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "lojista-01", new DateOnly(2026, 4, 16), EntryType.Credito, 100m, "Venda", "PDV", processedAt))
        };

        var reconstruido = projector.Rebuild("lojista-01", new DateOnly(2026, 4, 16), eventos, processedAt);

        Assert.Equal(100m, reconstruido.TotalCredits);
        Assert.Equal(20m, reconstruido.TotalDebits);
        Assert.Equal(80m, reconstruido.Balance);
        Assert.Equal(2L, reconstruido.LastSequenceId);
    }
}
