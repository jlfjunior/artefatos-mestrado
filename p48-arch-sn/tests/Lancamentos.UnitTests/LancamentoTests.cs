using FluentAssertions;
using Lancamentos.Domain;
using Xunit;

namespace Lancamentos.UnitTests;

public class LancamentoTests
{
    [Fact]
    public void Registrar_credito_deve_somar_no_saldo()
    {
        var lanc = Lancamento.Registrar(TipoLancamento.Credito, 100m, new DateOnly(2026, 6, 19), "venda");

        lanc.ImpactoNoSaldo().Should().Be(100m);
        lanc.Id.Should().NotBeEmpty();
        lanc.Descricao.Should().Be("venda");
    }

    [Fact]
    public void Registrar_debito_deve_subtrair_no_saldo()
    {
        var lanc = Lancamento.Registrar(TipoLancamento.Debito, 40m, new DateOnly(2026, 6, 19), null);

        lanc.ImpactoNoSaldo().Should().Be(-40m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Registrar_com_valor_nao_positivo_deve_falhar(decimal valor)
    {
        var acao = () => Lancamento.Registrar(TipoLancamento.Credito, valor, new DateOnly(2026, 6, 19), null);

        acao.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void Registrar_com_tipo_invalido_deve_falhar()
    {
        var acao = () => Lancamento.Registrar((TipoLancamento)99, 10m, new DateOnly(2026, 6, 19), null);

        acao.Should().Throw<DomainException>().WithMessage("*inválido*");
    }

    [Fact]
    public void Descricao_em_branco_vira_nula()
    {
        var lanc = Lancamento.Registrar(TipoLancamento.Credito, 10m, new DateOnly(2026, 6, 19), "   ");

        lanc.Descricao.Should().BeNull();
    }

    [Fact]
    public void Descricao_com_exatamente_200_caracteres_e_aceita()
    {
        var descricaoNoLimite = new string('x', 200);

        var lanc = Lancamento.Registrar(TipoLancamento.Credito, 10m, new DateOnly(2026, 6, 19), descricaoNoLimite);

        lanc.Descricao.Should().HaveLength(200);
    }

    [Fact]
    public void Descricao_acima_de_200_caracteres_deve_falhar()
    {
        var descricaoLonga = new string('x', 201);

        var acao = () => Lancamento.Registrar(TipoLancamento.Credito, 10m, new DateOnly(2026, 6, 19), descricaoLonga);

        acao.Should().Throw<DomainException>().WithMessage("*200 caracteres*");
    }
}
