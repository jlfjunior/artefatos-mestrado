using FluentAssertions;
using FluxoCaixa.Lancamentos.Application.Queries;
using Xunit;
using FluxoCaixa.Lancamentos.Domain.Entities;
using FluxoCaixa.Lancamentos.Domain.Repositories;
using Moq;

namespace FluxoCaixa.Lancamentos.Tests.Application;

public class GetLancamentosQueryHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly GetLancamentosQueryHandler _handler;

    public GetLancamentosQueryHandlerTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _handler = new GetLancamentosQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DataComLancamentos_DeveRetornarListaMapeada()
    {
        var data = new DateOnly(2026, 3, 30);
        var dataHoraUtc = new DateTimeOffset(2026, 3, 30, 13, 0, 0, TimeSpan.Zero);
        var lancamentos = new List<Lancamento>
        {
            Lancamento.Criar(TipoLancamento.Credito, 100m, "Venda 1", dataHoraUtc),
            Lancamento.Criar(TipoLancamento.Debito, 50m, "Compra 1", dataHoraUtc)
        };

        _repositoryMock
            .Setup(r => r.GetByDataAsync(data, "America/Sao_Paulo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        var query = new GetLancamentosQuery("2026-03-30", "America/Sao_Paulo");
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result[0].Tipo.Should().Be("CREDITO");
        result[0].Valor.Should().Be(100m);
        result[1].Tipo.Should().Be("DEBITO");
        result[1].Valor.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_DataSemLancamentos_DeveRetornarListaVazia()
    {
        _repositoryMock
            .Setup(r => r.GetByDataAsync(It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Lancamento>());

        var query = new GetLancamentosQuery("2099-01-01");
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FormatoDataInvalido_DeveLancarArgumentException()
    {
        var query = new GetLancamentosQuery("30/03/2026");

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Formato de data inválido. Use yyyy-MM-dd");
    }

    [Fact]
    public async Task Handle_DevePassarFusoHorarioCorretoParaRepositorio()
    {
        _repositoryMock
            .Setup(r => r.GetByDataAsync(It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Lancamento>());

        var query = new GetLancamentosQuery("2026-03-30", "America/Manaus");
        await _handler.Handle(query, CancellationToken.None);

        _repositoryMock.Verify(r => r.GetByDataAsync(
            new DateOnly(2026, 3, 30),
            "America/Manaus",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
