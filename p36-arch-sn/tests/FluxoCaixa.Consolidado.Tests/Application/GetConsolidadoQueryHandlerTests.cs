using FluentAssertions;
using FluxoCaixa.Consolidado.Application.Queries;
using FluxoCaixa.Consolidado.Domain.Entities;
using FluxoCaixa.Consolidado.Domain.Repositories;
using Moq;
using Xunit;

namespace FluxoCaixa.Consolidado.Tests.Application;

public class GetConsolidadoQueryHandlerTests
{
    private readonly Mock<IConsolidadoRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly GetConsolidadoQueryHandler _handler;

    public GetConsolidadoQueryHandlerTests()
    {
        _repositoryMock = new Mock<IConsolidadoRepository>();
        _cacheMock = new Mock<ICacheService>();
        _handler = new GetConsolidadoQueryHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_CacheHit_DeveRetornarCacheSemAcessarBanco()
    {
        var cachedResponse = new ConsolidadoResponse(
            new DateOnly(2026, 3, 30), 500m, 200m, 300m, 3, DateTimeOffset.UtcNow);

        _cacheMock
            .Setup(c => c.GetAsync<ConsolidadoResponse>("consolidado:2026-03-30"))
            .ReturnsAsync(cachedResponse);

        var result = await _handler.Handle(new GetConsolidadoQuery("2026-03-30"), CancellationToken.None);

        result.Should().Be(cachedResponse);
        _repositoryMock.Verify(r => r.GetByDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CacheMiss_DeveAcessarBancoEArmazenarNoCache()
    {
        var data = new DateOnly(2026, 3, 30);
        var entity = new ConsolidadoDiario(data);
        entity.AplicarLancamento("CREDITO", 300m);
        entity.AplicarLancamento("DEBITO", 100m);

        _cacheMock
            .Setup(c => c.GetAsync<ConsolidadoResponse>(It.IsAny<string>()))
            .ReturnsAsync((ConsolidadoResponse?)null);

        _repositoryMock
            .Setup(r => r.GetByDataAsync(data, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _handler.Handle(new GetConsolidadoQuery("2026-03-30"), CancellationToken.None);

        result.TotalCreditos.Should().Be(300m);
        result.TotalDebitos.Should().Be(100m);
        result.SaldoConsolidado.Should().Be(200m);

        _cacheMock.Verify(c => c.SetAsync(
            "consolidado:2026-03-30",
            It.IsAny<ConsolidadoResponse>(),
            TimeSpan.FromSeconds(60)), Times.Once);
    }

    [Fact]
    public async Task Handle_DiaSemLancamentos_DeveRetornarZerado()
    {
        _cacheMock
            .Setup(c => c.GetAsync<ConsolidadoResponse>(It.IsAny<string>()))
            .ReturnsAsync((ConsolidadoResponse?)null);

        _repositoryMock
            .Setup(r => r.GetByDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoDiario?)null);

        var result = await _handler.Handle(new GetConsolidadoQuery("2026-03-30"), CancellationToken.None);

        result.TotalCreditos.Should().Be(0m);
        result.TotalDebitos.Should().Be(0m);
        result.SaldoConsolidado.Should().Be(0m);
        result.QuantidadeLancamentos.Should().Be(0);
    }

    [Fact]
    public async Task Handle_FormatoDataInvalido_DeveLancarArgumentException()
    {
        var act = async () => await _handler.Handle(new GetConsolidadoQuery("30-03-2026"), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("Formato de data inválido. Use yyyy-MM-dd");
    }

    [Fact]
    public async Task Handle_DeveSalvarNoCacheComTTL60Segundos()
    {
        _cacheMock
            .Setup(c => c.GetAsync<ConsolidadoResponse>(It.IsAny<string>()))
            .ReturnsAsync((ConsolidadoResponse?)null);

        _repositoryMock
            .Setup(r => r.GetByDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidadoDiario?)null);

        await _handler.Handle(new GetConsolidadoQuery("2026-03-30"), CancellationToken.None);

        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<ConsolidadoResponse>(),
            TimeSpan.FromSeconds(60)), Times.Once);
    }
}
