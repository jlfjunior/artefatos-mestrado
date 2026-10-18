using FluentAssertions;
using FluxoCaixa.Lancamentos.Domain.Entities;
using FluxoCaixa.Lancamentos.Domain.Exceptions;
using Xunit;

namespace FluxoCaixa.Lancamentos.Tests.Domain;

public class LancamentoTests
{
    [Fact]
    public void Criar_CreditoComValorPositivo_DeveRetornarLancamentoValido()
    {
        var dataHora = new DateTimeOffset(2026, 3, 30, 10, 0, 0, TimeSpan.FromHours(-3));

        var lancamento = Lancamento.Criar(TipoLancamento.Credito, 150.00m, "Venda balcão", dataHora);

        lancamento.Id.Should().NotBeEmpty();
        lancamento.Tipo.Should().Be(TipoLancamento.Credito);
        lancamento.Valor.Should().Be(150.00m);
        lancamento.Descricao.Should().Be("Venda balcão");
        lancamento.DataHora.Offset.Should().Be(TimeSpan.Zero); // Armazenado em UTC
    }

    [Fact]
    public void Criar_DebitoComValorPositivo_DeveRetornarLancamentoValido()
    {
        var lancamento = Lancamento.Criar(TipoLancamento.Debito, 50.00m, "Fornecedor", DateTimeOffset.UtcNow);

        lancamento.Tipo.Should().Be(TipoLancamento.Debito);
        lancamento.Valor.Should().Be(50.00m);
    }

    [Fact]
    public void Criar_ComValorZero_DeveLancarDomainException()
    {
        var act = () => Lancamento.Criar(TipoLancamento.Credito, 0m, "Teste", DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
           .WithMessage("Valor deve ser positivo.");
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveLancarDomainException()
    {
        var act = () => Lancamento.Criar(TipoLancamento.Credito, -100m, "Teste", DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
           .WithMessage("Valor deve ser positivo.");
    }

    [Fact]
    public void Criar_ComDescricaoVazia_DeveLancarDomainException()
    {
        var act = () => Lancamento.Criar(TipoLancamento.Credito, 100m, "", DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
           .WithMessage("Descrição é obrigatória.");
    }

    [Fact]
    public void Criar_ComDescricaoNula_DeveLancarDomainException()
    {
        var act = () => Lancamento.Criar(TipoLancamento.Credito, 100m, null!, DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
           .WithMessage("Descrição é obrigatória.");
    }

    [Fact]
    public void Criar_DataHoraDeveSerNormalizadaParaUTC()
    {
        var dataHoraLocal = new DateTimeOffset(2026, 3, 30, 10, 0, 0, TimeSpan.FromHours(-3));

        var lancamento = Lancamento.Criar(TipoLancamento.Credito, 100m, "Teste", dataHoraLocal);

        lancamento.DataHora.Offset.Should().Be(TimeSpan.Zero);
        lancamento.DataHora.Should().Be(dataHoraLocal.ToUniversalTime());
    }

    [Fact]
    public void Criar_CriadoEmDeveSerUtc()
    {
        var antes = DateTimeOffset.UtcNow;

        var lancamento = Lancamento.Criar(TipoLancamento.Credito, 100m, "Teste", DateTimeOffset.UtcNow);

        lancamento.CriadoEm.Should().BeOnOrAfter(antes);
        lancamento.CriadoEm.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void DataConsolidado_HorarioMeianoiteUtcRetornaDiaAnteriorEmSaoPaulo()
    {
        // 00:30 UTC = 21:30 do dia anterior em BRT (UTC-3)
        var dataHoraUtc = new DateTimeOffset(2026, 3, 30, 0, 30, 0, TimeSpan.Zero);
        var lancamento = Lancamento.Criar(TipoLancamento.Credito, 100m, "Teste", dataHoraUtc);

        var dataConsolidado = lancamento.DataConsolidado("America/Sao_Paulo");

        dataConsolidado.Should().Be(new DateOnly(2026, 3, 29));
    }

    [Fact]
    public void DataConsolidado_HorarioDiurnoUtcRetornaMesmoDiaEmSaoPaulo()
    {
        // 15:00 UTC = 12:00 em BRT (UTC-3) — mesmo dia
        var dataHoraUtc = new DateTimeOffset(2026, 3, 30, 15, 0, 0, TimeSpan.Zero);
        var lancamento = Lancamento.Criar(TipoLancamento.Credito, 100m, "Teste", dataHoraUtc);

        var dataConsolidado = lancamento.DataConsolidado("America/Sao_Paulo");

        dataConsolidado.Should().Be(new DateOnly(2026, 3, 30));
    }

    [Fact]
    public void Criar_IdDeveSerUnico()
    {
        var l1 = Lancamento.Criar(TipoLancamento.Credito, 100m, "L1", DateTimeOffset.UtcNow);
        var l2 = Lancamento.Criar(TipoLancamento.Credito, 100m, "L2", DateTimeOffset.UtcNow);

        l1.Id.Should().NotBe(l2.Id);
    }
}
