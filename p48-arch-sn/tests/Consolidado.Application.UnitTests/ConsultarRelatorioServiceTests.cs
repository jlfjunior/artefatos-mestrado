using Consolidado.Application;
using Consolidado.Domain;
using FluentAssertions;
using Xunit;

namespace Consolidado.Application.UnitTests;

public class ConsultarRelatorioServiceTests
{
    private readonly SaldoRepositorioFake _saldos = new();

    private ConsultarRelatorioService CriarServico() => new(_saldos);

    private static SaldoDiario Saldo(DateOnly data, decimal credito, decimal debito)
    {
        var saldo = new SaldoDiario(data);
        if (credito > 0) saldo.AplicarCredito(credito);
        if (debito > 0) saldo.AplicarDebito(debito);
        return saldo;
    }

    [Fact]
    public async Task Relatorio_agrega_os_dias_do_periodo_ordenados()
    {
        _saldos.Semear(Saldo(new DateOnly(2026, 6, 2), credito: 50m, debito: 0m));   // dentro, saldo 50
        _saldos.Semear(Saldo(new DateOnly(2026, 6, 1), credito: 100m, debito: 40m)); // dentro, saldo 60
        _saldos.Semear(Saldo(new DateOnly(2026, 7, 1), credito: 999m, debito: 0m));  // fora do período

        var relatorio = await CriarServico().GerarAsync(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30));

        relatorio.Dias.Should().HaveCount(2);
        relatorio.Dias.Select(d => d.Data).Should().ContainInOrder(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 2));
        relatorio.TotalCreditos.Should().Be(150m);
        relatorio.TotalDebitos.Should().Be(40m);
        relatorio.Saldo.Should().Be(110m);
    }

    [Fact]
    public async Task Relatorio_de_periodo_sem_movimento_vem_vazio_e_zerado()
    {
        var relatorio = await CriarServico().GerarAsync(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        relatorio.Dias.Should().BeEmpty();
        relatorio.TotalCreditos.Should().Be(0m);
        relatorio.TotalDebitos.Should().Be(0m);
        relatorio.Saldo.Should().Be(0m);
    }

    [Fact]
    public async Task Data_final_antes_da_inicial_deve_falhar()
    {
        var acao = async () => await CriarServico().GerarAsync(new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 1));

        await acao.Should().ThrowAsync<ArgumentException>();
    }
}
