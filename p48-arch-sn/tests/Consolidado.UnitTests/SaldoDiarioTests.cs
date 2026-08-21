using Consolidado.Domain;
using FluentAssertions;
using Xunit;

namespace Consolidado.UnitTests;

public class SaldoDiarioTests
{
    private static readonly DateOnly Dia = new(2026, 6, 19);

    [Fact]
    public void Saldo_novo_comeca_zerado()
    {
        var saldo = new SaldoDiario(Dia);

        saldo.Saldo.Should().Be(0);
        saldo.TotalCreditos.Should().Be(0);
        saldo.TotalDebitos.Should().Be(0);
    }

    [Fact]
    public void Construtor_guarda_a_data_e_marca_atualizacao()
    {
        var antes = DateTime.UtcNow;

        var saldo = new SaldoDiario(Dia);

        saldo.Data.Should().Be(Dia);
        saldo.AtualizadoEmUtc.Should().BeOnOrAfter(antes);
    }

    [Fact]
    public void Apenas_creditos_ja_refletem_no_saldo()
    {
        var saldo = new SaldoDiario(Dia);

        saldo.AplicarCredito(100m);
        saldo.AplicarCredito(50m);

        saldo.TotalCreditos.Should().Be(150m);
        saldo.TotalDebitos.Should().Be(0m);
        saldo.Saldo.Should().Be(150m);
    }

    [Fact]
    public void Apenas_debitos_ja_refletem_no_saldo()
    {
        var saldo = new SaldoDiario(Dia);

        saldo.AplicarDebito(40m);
        saldo.AplicarDebito(20m);

        saldo.TotalDebitos.Should().Be(60m);
        saldo.TotalCreditos.Should().Be(0m);
        saldo.Saldo.Should().Be(-60m);
    }

    [Fact]
    public void Creditos_e_debitos_sao_consolidados_corretamente()
    {
        var saldo = new SaldoDiario(Dia);

        saldo.AplicarCredito(100m);
        saldo.AplicarCredito(50m);
        saldo.AplicarDebito(30m);

        saldo.TotalCreditos.Should().Be(150m);
        saldo.TotalDebitos.Should().Be(30m);
        saldo.Saldo.Should().Be(120m);
    }

    [Fact]
    public void Saldo_pode_ficar_negativo()
    {
        var saldo = new SaldoDiario(Dia);

        saldo.AplicarDebito(80m);

        saldo.Saldo.Should().Be(-80m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Credito_nao_positivo_e_rejeitado(decimal valor)
    {
        var saldo = new SaldoDiario(Dia);

        var acao = () => saldo.AplicarCredito(valor);

        acao.Should().Throw<ArgumentException>().WithMessage("*positivo*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Debito_nao_positivo_e_rejeitado(decimal valor)
    {
        var saldo = new SaldoDiario(Dia);

        var acao = () => saldo.AplicarDebito(valor);

        acao.Should().Throw<ArgumentException>().WithMessage("*positivo*");
    }
}
