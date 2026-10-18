using ControleLancamentos.Application.Lancamentos.Dtos;
using ControleLancamentos.Application.Lancamentos.Eventos;
using ControleLancamentos.Application.Lancamentos.Repositorios;
using ControleLancamentos.Application.Lancamentos.Services;
using ControleLancamentos.Domain.Lancamentos;
using NSubstitute;

namespace ControleLancamentos.Application.Tests.Lancamentos.Services;

[TestFixture]
public class CriarLancamentoServiceTests
{
    [Test]
    public async Task ExecutarAsync_QuandoDadosForemValidos_DevePersistirEPublicarEvento()
    {
        // Arrange
        var repositorio = Substitute.For<ILancamentoRepositorio>();
        var eventBus = Substitute.For<ILancamentoCriadoEventBus>();
        var service = new CriarLancamentoService(repositorio, eventBus);
        var request = new CriarLancamentoRequest
        {
            Valor = 100m,
            Tipo = TipoLancamento.Credito,
            DataLancamento = new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc),
            Descricao = "Recebimento"
        };

        // Act
        var response = await service.ExecutarAsync(request);

        // Assert
        await repositorio.Received(1).AdicionarAsync(
            Arg.Is<Lancamento>(x =>
                x.Id == response.Id &&
                x.Valor == request.Valor &&
                x.Tipo == request.Tipo &&
                x.DataLancamento == request.DataLancamento &&
                x.Descricao == request.Descricao),
            Arg.Any<CancellationToken>());
        await repositorio.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await eventBus.Received(1).PublicarAsync(
            Arg.Is<LancamentoCriadoEvent>(x =>
                x.LancamentoId == response.Id &&
                x.Valor == request.Valor &&
                x.Tipo == request.Tipo &&
                x.DataReferencia == DateOnly.FromDateTime(request.DataLancamento)),
            Arg.Any<CancellationToken>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task ExecutarAsync_QuandoValorForInvalido_DeveLancarExcecao(decimal valorInvalido)
    {
        // Arrange
        var repositorio = Substitute.For<ILancamentoRepositorio>();
        var eventBus = Substitute.For<ILancamentoCriadoEventBus>();
        var service = new CriarLancamentoService(repositorio, eventBus);
        var request = new CriarLancamentoRequest
        {
            Valor = valorInvalido,
            Tipo = TipoLancamento.Debito,
            DataLancamento = new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc),
            Descricao = "Teste inválido"
        };

        // Act
        async Task Acao() => await service.ExecutarAsync(request);

        // Assert
        Assert.That(Acao, Throws.TypeOf<ArgumentOutOfRangeException>());
        await repositorio.Received(0).AdicionarAsync(Arg.Any<Lancamento>(), Arg.Any<CancellationToken>());
        await repositorio.Received(0).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await eventBus.Received(0).PublicarAsync(Arg.Any<LancamentoCriadoEvent>(), Arg.Any<CancellationToken>());
    }
}
