using FluentAssertions;
using FluxoCaixa.Consolidado.Domain.Entities;
using Xunit;

namespace FluxoCaixa.Consolidado.Tests.Domain;

public class ConsolidadoDiarioTests
{
    [Fact]
    public void AplicarLancamento_Credito_DeveIncrementarTotalCreditos()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));

        consolidado.AplicarLancamento("CREDITO", 150.00m);

        consolidado.TotalCreditos.Should().Be(150.00m);
        consolidado.TotalDebitos.Should().Be(0m);
        consolidado.QuantidadeLancamentos.Should().Be(1);
    }

    [Fact]
    public void AplicarLancamento_Debito_DeveIncrementarTotalDebitos()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));

        consolidado.AplicarLancamento("DEBITO", 80.00m);

        consolidado.TotalDebitos.Should().Be(80.00m);
        consolidado.TotalCreditos.Should().Be(0m);
        consolidado.QuantidadeLancamentos.Should().Be(1);
    }

    [Fact]
    public void SaldoConsolidado_DeveSerCreditosMinusDebitos()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));

        consolidado.AplicarLancamento("CREDITO", 500m);
        consolidado.AplicarLancamento("DEBITO", 200m);

        consolidado.SaldoConsolidado.Should().Be(300m);
    }

    [Fact]
    public void AplicarLancamento_MultiplosLancamentos_DeveAcumularCorretamente()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));

        consolidado.AplicarLancamento("CREDITO", 100m);
        consolidado.AplicarLancamento("CREDITO", 200m);
        consolidado.AplicarLancamento("DEBITO", 50m);
        consolidado.AplicarLancamento("DEBITO", 30m);

        consolidado.TotalCreditos.Should().Be(300m);
        consolidado.TotalDebitos.Should().Be(80m);
        consolidado.SaldoConsolidado.Should().Be(220m);
        consolidado.QuantidadeLancamentos.Should().Be(4);
    }

    [Fact]
    public void AplicarLancamento_DeveAtualizarUltimaAtualizacao()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));
        var antes = DateTimeOffset.UtcNow;

        consolidado.AplicarLancamento("CREDITO", 100m);

        consolidado.UltimaAtualizacao.Should().BeOnOrAfter(antes);
    }

    [Fact]
    public void NovoConsolidado_DeveIniciarComValoresZerados()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));

        consolidado.TotalCreditos.Should().Be(0m);
        consolidado.TotalDebitos.Should().Be(0m);
        consolidado.SaldoConsolidado.Should().Be(0m);
        consolidado.QuantidadeLancamentos.Should().Be(0);
    }

    [Fact]
    public void SaldoConsolidado_QuandoDebitosMaioresQueCreditos_DeveSerNegativo()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));

        consolidado.AplicarLancamento("CREDITO", 100m);
        consolidado.AplicarLancamento("DEBITO", 300m);

        consolidado.SaldoConsolidado.Should().Be(-200m);
    }

    [Fact]
    public void AplicarLancamento_TipoMinusculo_DeveSerReconhecido()
    {
        var consolidado = new ConsolidadoDiario(new DateOnly(2026, 3, 30));

        consolidado.AplicarLancamento("credito", 100m);

        consolidado.TotalCreditos.Should().Be(100m);
    }
}
