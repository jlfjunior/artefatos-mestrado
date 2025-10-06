using CashFlow.Application.Commands;
using CashFlow.Application.UseCases;
using CashFlow.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;


namespace CashFlow.UnitTest.Application.UseCases;

public class CancelTransactionUseCaseTests : FixtureUseCase<CancelTransactionUseCase>
{
    private readonly Mock<ICashFlowService> _service = new();
    private readonly CancelTransactionUseCase _useCase;

    public CancelTransactionUseCaseTests()
    {
        _useCase = new CancelTransactionUseCase(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResponse()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _service.Setup(service => service.ReverseTransaction(transactionId))
            .ReturnsAsync(new Transaction(Guid.NewGuid(), 100M, TransactionType.Credit));

        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        // Act
        var (isSuccess, response, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(isSuccess);
        Assert.Null(error);
        Assert.NotEqual(default, response);

        _service.Verify(service => service.ReverseTransaction(transactionId), Times.Once);

        LoggerVerify(LogLevel.Information, "was successfully canceled or reversed.");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsFailResponse()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _service.Setup(service => service.ReverseTransaction(transactionId))
            .ReturnsAsync(default(Transaction));

        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        // Act
        var (isSuccess, response, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(TransactionNotFound, error.ErrorCode);
        Assert.Equal(default, response);
        _service.Verify(service => service.ReverseTransaction(transactionId), Times.Once);

        LoggerVerify(LogLevel.Error, "cannot be canceled or reversed");
    }

    [Fact]
    public async Task Handle_TransactionNotFoundException_ReturnsFailResponse()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var exceptionMessage = "Transaction not found.";

        _service.Setup(service => service.ReverseTransaction(transactionId))
            .ThrowsAsync(new TransactionNotFoundException(exceptionMessage));

        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        // Act
        var (isSuccess, response, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(TransactionNotFound, error.ErrorCode);
        Assert.Equal(default, response);
        _service.Verify(service => service.ReverseTransaction(transactionId), Times.Once);

        LoggerVerify(LogLevel.Error, "Transaction not found.");
    }

    [Fact]
    public async Task Handle_InternalException_ReturnsFailResponse()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _service.Setup(service => service.ReverseTransaction(transactionId))
            .ThrowsAsync(new Exception());

        var command = new CancelTransactionCommand
        {
            TransactionId = transactionId
        };

        // Act
        var (isSuccess, response, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(InternalError, error.ErrorCode);
        Assert.Equal(default, response);
        _service.Verify(service => service.ReverseTransaction(transactionId), Times.Once);
        LoggerVerify(LogLevel.Critical, "Error canceling transaction.");
    }
}