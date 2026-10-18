using FluentAssertions;
using FluxoCaixa.Consolidado.Application.Consumers;
using FluxoCaixa.Consolidado.Domain.Repositories;
using FluxoCaixa.Lancamentos.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluxoCaixa.Consolidado.Tests.Application;

public class LancamentoCriadoEventConsumerTests
{
    private readonly Mock<IConsolidadoRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<LancamentoCriadoEventConsumer>> _loggerMock;
    private readonly LancamentoCriadoEventConsumer _consumer;

    public LancamentoCriadoEventConsumerTests()
    {
        _repositoryMock = new Mock<IConsolidadoRepository>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<LancamentoCriadoEventConsumer>>();
        _consumer = new LancamentoCriadoEventConsumer(_repositoryMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_EventoNovo_DeveAplicarLancamentoAtomicoEInvalidarCache()
    {
        var eventId = Guid.NewGuid();
        var evt = new LancamentoCriadoEvent
        {
            EventId = eventId,
            LancamentoId = Guid.NewGuid(),
            Tipo = "CREDITO",
            Valor = 300m,
            DataHora = new DateTimeOffset(2026, 3, 30, 13, 0, 0, TimeSpan.Zero)
        };

        _repositoryMock.Setup(r => r.HasEventBeenProcessedAsync(eventId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AplicarLancamentoAtomicAsync(
                It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _consumer.Consume(CreateContext(evt));

        _repositoryMock.Verify(r => r.AplicarLancamentoAtomicAsync(
            new DateOnly(2026, 3, 30), "CREDITO", 300m, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.MarkEventAsProcessedAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains("2026-03-30"))), Times.Once);
    }

    [Fact]
    public async Task Consume_EventoDebito_DeveAplicarLancamentoAtomicoComTipoDebito()
    {
        var eventId = Guid.NewGuid();
        var evt = new LancamentoCriadoEvent
        {
            EventId = eventId,
            LancamentoId = Guid.NewGuid(),
            Tipo = "DEBITO",
            Valor = 150m,
            DataHora = new DateTimeOffset(2026, 3, 30, 15, 0, 0, TimeSpan.Zero)
        };

        _repositoryMock.Setup(r => r.HasEventBeenProcessedAsync(eventId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AplicarLancamentoAtomicAsync(
                It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _consumer.Consume(CreateContext(evt));

        _repositoryMock.Verify(r => r.AplicarLancamentoAtomicAsync(
            new DateOnly(2026, 3, 30), "DEBITO", 150m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_EventoDuplicado_DeveIgnorar()
    {
        var eventId = Guid.NewGuid();
        var evt = new LancamentoCriadoEvent
        {
            EventId = eventId,
            Tipo = "CREDITO",
            Valor = 100m,
            DataHora = DateTimeOffset.UtcNow
        };

        _repositoryMock.Setup(r => r.HasEventBeenProcessedAsync(eventId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        await _consumer.Consume(CreateContext(evt));

        _repositoryMock.Verify(r => r.AplicarLancamentoAtomicAsync(
            It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.MarkEventAsProcessedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Consume_DataHoraUtcMeiaNoite_DeveMapeiaParaDiaAnteriorEmSaoPaulo()
    {
        // 00:30 UTC = 21:30 do dia anterior em BRT (UTC-3)
        var eventId = Guid.NewGuid();
        var evt = new LancamentoCriadoEvent
        {
            EventId = eventId,
            Tipo = "CREDITO",
            Valor = 200m,
            DataHora = new DateTimeOffset(2026, 3, 30, 0, 30, 0, TimeSpan.Zero)
        };

        _repositoryMock.Setup(r => r.HasEventBeenProcessedAsync(eventId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AplicarLancamentoAtomicAsync(
                It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _consumer.Consume(CreateContext(evt));

        // Deve consolidar no dia 29/03, não 30/03 (por causa do fuso -3)
        _repositoryMock.Verify(r => r.AplicarLancamentoAtomicAsync(
            new DateOnly(2026, 3, 29), "CREDITO", 200m, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ConsumeContext<LancamentoCriadoEvent> CreateContext(LancamentoCriadoEvent evt)
    {
        var mock = new Mock<ConsumeContext<LancamentoCriadoEvent>>();
        mock.Setup(c => c.Message).Returns(evt);
        return mock.Object;
    }
}
