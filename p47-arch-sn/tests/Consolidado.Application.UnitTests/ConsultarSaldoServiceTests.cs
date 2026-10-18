using Consolidado.Application;
using Consolidado.Application.Dtos;
using Consolidado.Domain;
using FluentAssertions;
using Xunit;

namespace Consolidado.Application.UnitTests;

/// <summary>
/// Testes do caminho quente de leitura (cache-aside). É o endpoint que recebe os
/// 50 req/s de pico, então quero garantir que o cache é usado quando há hit e
/// repovoado no miss — sem ir ao banco à toa.
/// </summary>
public class ConsultarSaldoServiceTests
{
    private readonly SaldoRepositorioFake _saldos = new();
    private readonly SaldoCacheFake _cache = new();

    private static readonly DateOnly Dia = new(2026, 6, 19);

    private ConsultarSaldoService CriarServico() => new(_saldos, _cache);

    [Fact]
    public async Task Cache_hit_deve_retornar_sem_consultar_o_banco()
    {
        var dto = new SaldoDiarioDto(Dia, 100m, 30m, 70m, DateTime.UtcNow);
        _cache.Semear(dto);
        var servico = CriarServico();

        var resultado = await servico.ConsultarAsync(Dia);

        resultado.Should().BeEquivalentTo(dto);
        _cache.Gravacoes.Should().Be(0, "num hit não repovoamos o cache");
    }

    [Fact]
    public async Task Cache_miss_deve_ler_do_banco_e_repovoar_o_cache()
    {
        var saldo = new SaldoDiario(Dia);
        saldo.AplicarCredito(120m);
        saldo.AplicarDebito(20m);
        _saldos.Semear(saldo);
        var servico = CriarServico();

        var resultado = await servico.ConsultarAsync(Dia);

        resultado.Should().NotBeNull();
        resultado!.Saldo.Should().Be(100m);
        resultado.TotalCreditos.Should().Be(120m);
        resultado.TotalDebitos.Should().Be(20m);
        _cache.Gravacoes.Should().Be(1, "no miss repovoamos o cache para a próxima leitura");
    }

    [Fact]
    public async Task Dia_sem_lancamentos_deve_retornar_nulo()
    {
        var servico = CriarServico();

        var resultado = await servico.ConsultarAsync(Dia);

        resultado.Should().BeNull();
        _cache.Gravacoes.Should().Be(0);
    }
}
