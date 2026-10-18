using Consolidado.Application;
using Consolidado.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Consolidado.Application.UnitTests;

/// <summary>
/// Testes de unidade da projeção do saldo. Aqui mora a regra mais delicada do
/// read side: idempotência por LancamentoId. Os dublês me deixam provar isso sem
/// banco — e dão ao Stryker mutações ricas para atacar na Application.
/// </summary>
public class AtualizarSaldoServiceTests
{
    private readonly SaldoRepositorioFake _saldos = new();
    private readonly ProcessadosStoreFake _processados = new();
    private readonly SaldoCacheFake _cache = new();
    private readonly UnitOfWorkFake _uow = new();

    private static readonly DateOnly Dia = new(2026, 6, 19);

    private AtualizarSaldoService CriarServico()
        => new(_saldos, _processados, _cache, _uow, NullLogger<AtualizarSaldoService>.Instance);

    private static AtualizarSaldoCommand Comando(string tipo, decimal valor, Guid? id = null)
        => new(id ?? Guid.NewGuid(), tipo, valor, Dia);

    [Fact]
    public async Task Credito_novo_deve_criar_a_projecao_do_dia()
    {
        var servico = CriarServico();

        await servico.ExecutarAsync(Comando("Credito", 100m));

        var saldo = await _saldos.ObterPorDataAsync(Dia);
        saldo.Should().NotBeNull();
        saldo!.TotalCreditos.Should().Be(100m);
        saldo.Saldo.Should().Be(100m);
    }

    [Fact]
    public async Task Credito_e_debito_devem_compor_o_saldo()
    {
        var servico = CriarServico();

        await servico.ExecutarAsync(Comando("Credito", 100m));
        await servico.ExecutarAsync(Comando("Debito", 30m));

        var saldo = await _saldos.ObterPorDataAsync(Dia);
        saldo!.TotalCreditos.Should().Be(100m);
        saldo.TotalDebitos.Should().Be(30m);
        saldo.Saldo.Should().Be(70m);
    }

    [Theory]
    [InlineData("credito")]
    [InlineData("CREDITO")]
    [InlineData("Debito")]
    [InlineData("DEBITO")]
    public async Task Tipo_deve_ser_aceito_sem_diferenciar_maiusculas(string tipo)
    {
        var servico = CriarServico();

        var acao = async () => await servico.ExecutarAsync(Comando(tipo, 10m));

        await acao.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Tipo_desconhecido_deve_falhar()
    {
        var servico = CriarServico();

        var acao = async () => await servico.ExecutarAsync(Comando("Estorno", 10m));

        await acao.Should().ThrowAsync<InvalidOperationException>().WithMessage("*desconhecido*");
    }

    [Fact]
    public async Task Reentrega_do_mesmo_lancamento_nao_deve_dobrar_o_saldo()
    {
        var servico = CriarServico();
        var id = Guid.NewGuid();

        await servico.ExecutarAsync(Comando("Credito", 200m, id));
        await servico.ExecutarAsync(Comando("Credito", 200m, id));

        var saldo = await _saldos.ObterPorDataAsync(Dia);
        saldo!.Saldo.Should().Be(200m, "o consumidor é idempotente por LancamentoId");
        saldo.TotalCreditos.Should().Be(200m);
    }

    [Fact]
    public async Task Lancamento_ja_processado_nao_deve_tocar_em_saldo_cache_nem_commit()
    {
        var id = Guid.NewGuid();
        _processados.MarcarPreexistente(id);
        var servico = CriarServico();

        await servico.ExecutarAsync(Comando("Credito", 999m, id));

        _saldos.Salvamentos.Should().Be(0);
        _cache.Invalidacoes.Should().Be(0);
        _uow.Commits.Should().Be(0);
    }

    [Fact]
    public async Task Processamento_deve_invalidar_o_cache_do_dia()
    {
        var servico = CriarServico();

        await servico.ExecutarAsync(Comando("Credito", 50m));

        _cache.Invalidacoes.Should().Be(1);
    }

    [Fact]
    public async Task Processamento_deve_confirmar_a_transacao_uma_vez()
    {
        var servico = CriarServico();

        await servico.ExecutarAsync(Comando("Debito", 50m));

        _uow.Commits.Should().Be(1);
    }
}
