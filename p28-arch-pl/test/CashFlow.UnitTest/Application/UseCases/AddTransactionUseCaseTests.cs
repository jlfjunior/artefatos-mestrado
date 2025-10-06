using CashFlow.Application;
using CashFlow.Application.Commands;
using CashFlow.Application.UseCases;
using CashFlow.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CashFlow.UnitTest.Application.UseCases;

public class AddTransactionUseCaseTests : FixtureUseCase<AddTransactionUseCase>
{
    private readonly Mock<ICashFlowService> _domainService = new();

    private readonly AddTransactionUseCase _useCase;

    public AddTransactionUseCaseTests()
    {
        _useCase = new AddTransactionUseCase(_domainService.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResponse()
    {
        // Arrange
        var command = new AddTransactionDailyCommand
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Type = TransactionType.Credit
        };

        var transaction = new Transaction(command.AccountId, command.Amount, command.Type);

        _domainService.Setup(service =>
                service.AddTransaction(command.AccountId, command.Amount, command.Type))
            .ReturnsAsync(transaction);

        // Act
        var (isSuccess, transactionId, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(isSuccess);
        Assert.NotEqual(default, transactionId);
        Assert.Null(error);

        LoggerVerify(LogLevel.Information, "registered");
    }


    [Fact]
    public async Task Handle_CashFlowNotFoundException_ReturnsFailResponse()
    {
        // Arrange
        var command = new AddTransactionDailyCommand
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Type = TransactionType.Credit
        };

        _domainService.Setup(service =>
                service.AddTransaction(command.AccountId, command.Amount, command.Type))
            .ThrowsAsync(new CashFlowNotFoundException(""));

        // Act
        var (isSuccess, transactionId, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(default, transactionId);
        Assert.Equal(ErrorCode.CashFlowNotFound, error!.ErrorCode);

        LoggerVerify(LogLevel.Error, "");
    }

    [Fact]
    public async Task Handle_InternalException_ReturnsFailResponse()
    {
        // Arrange
        var command = new AddTransactionDailyCommand
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Type = TransactionType.Credit
        };

        _domainService.Setup(service =>
                service.AddTransaction(command.AccountId, command.Amount, command.Type))
            .ThrowsAsync(new Exception());

        // Act
        var (isSuccess, transactionId, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(default, transactionId);
        Assert.Equal(ErrorCode.InternalError, error!.ErrorCode);

        LoggerVerify(LogLevel.Critical, "Erro interno");
    }
}