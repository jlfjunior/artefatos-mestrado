using FluentAssertions;
using FluxoCaixa.Lancamentos.Application.Commands;
using Xunit;
using FluxoCaixa.Lancamentos.Domain.Entities;
using FluxoCaixa.Lancamentos.Domain.Events;
using FluxoCaixa.Lancamentos.Domain.Repositories;
using Moq;

namespace FluxoCaixa.Lancamentos.Tests.Application;

public class CreateLancamentoCommandHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _publisherMock;
    private readonly CreateLancamentoCommandHandler _handler;

    public CreateLancamentoCommandHandlerTests()
    {
        _repositoryMock = new Mock<ILancamentoRepository>();
        _publisherMock = new Mock<IEventPublisher>();
        _handler = new CreateLancamentoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_LancamentoCreditoValido_DeveSalvarEPublicarEvento()
    {
        var command = new CreateLancamentoCommand("CREDITO", 150.00m, "Venda balcão", DateTimeOffset.UtcNow);

        var result = await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        result.Id.Should().NotBeEmpty();
        result.Tipo.Should().Be("CREDITO");
        result.Valor.Should().Be(150.00m);
    }

    [Fact]
    public async Task Handle_LancamentoDebitoValido_DeveSalvarEPublicarEvento()
    {
        var command = new CreateLancamentoCommand("DEBITO", 80.00m, "Fornecedor", DateTimeOffset.UtcNow);

        var result = await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        result.Tipo.Should().Be("DEBITO");
    }

    [Fact]
    public async Task Handle_EventoPublicadoDeveConterDadosCorretos()
    {
        var dataHora = new DateTimeOffset(2026, 3, 30, 13, 0, 0, TimeSpan.Zero);
        var command = new CreateLancamentoCommand("CREDITO", 200m, "Teste evento", dataHora);

        LancamentoCriadoEvent? eventoCapturado = null;
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()))
            .Callback<LancamentoCriadoEvent, CancellationToken>((e, _) => eventoCapturado = e)
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        eventoCapturado.Should().NotBeNull();
        eventoCapturado!.Tipo.Should().Be("CREDITO");
        eventoCapturado.Valor.Should().Be(200m);
        eventoCapturado.DataHora.Offset.Should().Be(TimeSpan.Zero);
        eventoCapturado.EventId.Should().NotBeEmpty();
        eventoCapturado.LancamentoId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ResponseDeveConterDataHoraLocalCorreta()
    {
        // DataHora enviada com offset -3 (BRT)
        var dataHoraLocal = new DateTimeOffset(2026, 3, 30, 10, 0, 0, TimeSpan.FromHours(-3));
        var command = new CreateLancamentoCommand("CREDITO", 100m, "Teste", dataHoraLocal);

        var result = await _handler.Handle(command, CancellationToken.None);

        // DataHora no response deve ser UTC
        result.DataHora.Offset.Should().Be(TimeSpan.Zero);
        // DataHoraLocal deve ter o offset original
        result.DataHoraLocal.Offset.Should().Be(TimeSpan.FromHours(-3));
    }

    [Fact]
    public async Task Handle_QuandoPublisherFalha_DevePropagarExcecao()
    {
        var command = new CreateLancamentoCommand("CREDITO", 100m, "Teste", DateTimeOffset.UtcNow);
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<LancamentoCriadoEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("RabbitMQ indisponível"));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("RabbitMQ indisponível");
    }

    [Fact]
    public async Task Handle_TipoMinusculo_DeveSerAceitoENormalizado()
    {
        var command = new CreateLancamentoCommand("credito", 100m, "Teste", DateTimeOffset.UtcNow);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Tipo.Should().Be("CREDITO");
    }
}
