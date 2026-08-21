using ControleLancamentos.Application.Lancamentos.Dtos;
using ControleLancamentos.Application.Lancamentos.Eventos;
using ControleLancamentos.Application.Lancamentos.Repositorios;
using ControleLancamentos.Application.Lancamentos.Services;
using ControleLancamentos.Domain.Lancamentos;
using NSubstitute;

namespace ControleLancamentos.Application.Tests.Lancamentos.Services;

[TestFixture]
public class CalcularConsolidadoDiarioServiceTests
{
    [Test]
    public void ExecutarAsync_QuandoRequestForNulo_DeveLancarExcecao()
    {
        // Arrange
        var repositorio = Substitute.For<IConsolidadoDiarioRepositorio>();
        var service = new CalcularConsolidadoDiarioService(repositorio);

        // Act
        async Task Acao() => await service.ExecutarAsync(null!);

        // Assert
        Assert.That(Acao, Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ExecutarAsync_QuandoDataReferenciaNaoForInformada_DeveLancarExcecao()
    {
        // Arrange
        var repositorio = Substitute.For<IConsolidadoDiarioRepositorio>();
        var service = new CalcularConsolidadoDiarioService(repositorio);
        var request = new ConsolidadoDiarioRequest(null);

        // Act
        async Task Acao() => await service.ExecutarAsync(request);

        // Assert
        Assert.That(Acao, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task ExecutarAsync_QuandoNaoHouverConsolidadoParaAData_DeveRetornarValoresZerados()
    {
        // Arrange
        var dataReferencia = new DateOnly(2026, 5, 30);
        var repositorio = Substitute.For<IConsolidadoDiarioRepositorio>();
        repositorio.ObterPorDataAsync(dataReferencia, Arg.Any<CancellationToken>())
            .Returns((ConsolidadoDiario?)null);
        var service = new CalcularConsolidadoDiarioService(repositorio);

        // Act
        var resultado = await service.ExecutarAsync(new ConsolidadoDiarioRequest(dataReferencia));

        // Assert
        Assert.That(resultado.DataReferencia, Is.EqualTo(dataReferencia));
        Assert.That(resultado.TotalCreditos, Is.EqualTo(0m));
        Assert.That(resultado.TotalDebitos, Is.EqualTo(0m));
        Assert.That(resultado.Saldo, Is.EqualTo(0m));
        Assert.That(resultado.QuantidadeLancamentos, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecutarAsync_DadoListaDeEventosCreditoEDebitoNaMesmaData_DeveRetornarSaldoAcumuladoCorreto()
    {
        // Arrange
        var dataReferencia = new DateOnly(2026, 5, 30);
        var eventos = new[]
        {
            new LancamentoCriadoEvent(Guid.NewGuid(), dataReferencia, 100m, TipoLancamento.Credito),
            new LancamentoCriadoEvent(Guid.NewGuid(), dataReferencia, 30m, TipoLancamento.Debito)
        };

        var (totalCreditos, totalDebitos, saldo, quantidadeLancamentos) = AcumularEventos(eventos);
        var consolidado = CriarConsolidadoDiario(
            dataReferencia,
            totalCreditos,
            totalDebitos,
            saldo,
            quantidadeLancamentos);

        var repositorio = Substitute.For<IConsolidadoDiarioRepositorio>();
        repositorio.ObterPorDataAsync(dataReferencia, Arg.Any<CancellationToken>())
            .Returns(consolidado);

        var service = new CalcularConsolidadoDiarioService(repositorio);

        // Act
        var resultado = await service.ExecutarAsync(new ConsolidadoDiarioRequest(dataReferencia));

        // Assert
        Assert.That(resultado.TotalCreditos, Is.EqualTo(100m));
        Assert.That(resultado.TotalDebitos, Is.EqualTo(30m));
        Assert.That(resultado.Saldo, Is.EqualTo(70m));
        Assert.That(resultado.QuantidadeLancamentos, Is.EqualTo(2));

        await repositorio.Received(1).ObterPorDataAsync(dataReferencia, Arg.Any<CancellationToken>());
    }

    private static (decimal TotalCreditos, decimal TotalDebitos, decimal Saldo, int QuantidadeLancamentos) AcumularEventos(
        IEnumerable<LancamentoCriadoEvent> eventos)
    {
        decimal totalCreditos = 0m;
        decimal totalDebitos = 0m;
        decimal saldo = 0m;
        var quantidadeLancamentos = 0;

        foreach (var evento in eventos)
        {
            if (evento.Tipo == TipoLancamento.Credito)
            {
                totalCreditos += evento.Valor;
                saldo += evento.Valor;
            }
            else
            {
                totalDebitos += evento.Valor;
                saldo -= evento.Valor;
            }

            quantidadeLancamentos++;
        }

        return (totalCreditos, totalDebitos, saldo, quantidadeLancamentos);
    }

    private static ConsolidadoDiario CriarConsolidadoDiario(
        DateOnly dataReferencia,
        decimal totalCreditos,
        decimal totalDebitos,
        decimal saldo,
        int quantidadeLancamentos)
    {
        var consolidado = (ConsolidadoDiario)Activator.CreateInstance(
            typeof(ConsolidadoDiario),
            nonPublic: true)!;

        DefinirPropriedade(consolidado, nameof(ConsolidadoDiario.DataReferencia), dataReferencia);
        DefinirPropriedade(consolidado, nameof(ConsolidadoDiario.TotalCreditos), totalCreditos);
        DefinirPropriedade(consolidado, nameof(ConsolidadoDiario.TotalDebitos), totalDebitos);
        DefinirPropriedade(consolidado, nameof(ConsolidadoDiario.Saldo), saldo);
        DefinirPropriedade(consolidado, nameof(ConsolidadoDiario.QuantidadeLancamentos), quantidadeLancamentos);

        return consolidado;
    }

    private static void DefinirPropriedade<T>(ConsolidadoDiario consolidado, string nomePropriedade, T valor)
    {
        var propriedade = typeof(ConsolidadoDiario).GetProperty(nomePropriedade);
        propriedade!.SetValue(consolidado, valor);
    }
}
