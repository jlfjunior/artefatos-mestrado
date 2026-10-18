using ControleLancamentos.Domain.Lancamentos;

namespace ControleLancamentos.Domain.Tests.Lancamentos;

[TestFixture]
public class LancamentoRegrasTests
{
    [TestCase(0)]
    [TestCase(-10)]
    public void Criar_QuandoValorForMenorOuIgualAZero_DeveLancarExcecao(decimal valorInvalido)
    {
        // Arrange
        var id = Guid.NewGuid();
        var dataLancamento = DateTime.UtcNow;

        // Act
        var acao = () => LancamentoRegras.Criar(
            id,
            valorInvalido,
            TipoLancamento.Credito,
            dataLancamento,
            "Entrada inválida");

        // Assert
        Assert.That(acao, Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Criar_QuandoIdForVazio_DeveLancarExcecao()
    {
        // Arrange
        var dataLancamento = DateTime.UtcNow;

        // Act
        var acao = () => LancamentoRegras.Criar(
            Guid.Empty,
            100m,
            TipoLancamento.Credito,
            dataLancamento,
            "Lançamento sem id");

        // Assert
        Assert.That(acao, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Criar_QuandoDataLancamentoNaoForInformada_DeveLancarExcecao()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var acao = () => LancamentoRegras.Criar(
            id,
            100m,
            TipoLancamento.Credito,
            default,
            "Lançamento sem data");

        // Assert
        Assert.That(acao, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Criar_QuandoDescricaoExcederTamanhoMaximo_DeveLancarExcecao()
    {
        // Arrange
        var descricaoGrande = new string('A', 201);

        // Act
        var acao = () => LancamentoRegras.Criar(
            Guid.NewGuid(),
            50m,
            TipoLancamento.Debito,
            DateTime.UtcNow,
            descricaoGrande);

        // Assert
        Assert.That(acao, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Criar_QuandoDescricaoTiverEspacosNasPontas_DeveNormalizarDescricao()
    {
        // Arrange
        const string descricaoComEspacos = "  Compra no mercado  ";

        // Act
        var lancamento = LancamentoRegras.Criar(
            Guid.NewGuid(),
            30m,
            TipoLancamento.Debito,
            DateTime.UtcNow,
            descricaoComEspacos);

        // Assert
        Assert.That(lancamento.Descricao, Is.EqualTo("Compra no mercado"));
    }

    [Test]
    public void ObterValorComSinal_QuandoTipoForDebito_DeveRetornarValorNegativo()
    {
        // Arrange
        const decimal valor = 30m;

        // Act
        var resultado = LancamentoRegras.ObterValorComSinal(valor, TipoLancamento.Debito);

        // Assert
        Assert.That(resultado, Is.EqualTo(-30m));
    }

    [Test]
    public void ObterValorComSinal_QuandoTipoForCredito_DeveRetornarValorPositivo()
    {
        // Arrange
        const decimal valor = 100m;

        // Act
        var resultado = LancamentoRegras.ObterValorComSinal(valor, TipoLancamento.Credito);

        // Assert
        Assert.That(resultado, Is.EqualTo(100m));
    }
}
