using CashFlow.Application.Commands;
using CashFlow.Application.UseCases;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.UnitTest.Application.UseCases;

public class RegisterNewCashFlowUseCaseTests : FixtureUseCase<RegisterNewCashFlowUseCase>
{
    private readonly Mock<ICashFlowService> _domainService = new();

    private readonly RegisterNewCashFlowUseCase _useCase;

    public RegisterNewCashFlowUseCaseTests()
    {
        _useCase = new RegisterNewCashFlowUseCase(_domainService.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_Exception_ReturnsFailResponse()
    {
        // Arrange
        _domainService
            .Setup(s => s.RegisterNewAggregate(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Internal Error"))
            ;

        var command = new RegisterNewCashFlowCommand
        {
            AccountId = Guid.NewGuid()
        };

        // Act
        var (isSuccess, aggregateId, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal(default, aggregateId);
        Assert.Equal(InternalError, error.ErrorCode);

        LoggerVerify(LogLevel.Critical, "Erro interno");
    }

    [Fact]
    public async Task Handle_ReturnsSuccessResponse()
    {
        // Arrange
        _domainService
            .Setup(s => s.RegisterNewAggregate(It.IsAny<Guid>()))
            .ReturnsAsync(new CashFlowDailyAggregate(Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow.Date)))
            ;

        var command = new RegisterNewCashFlowCommand
        {
            AccountId = Guid.NewGuid()
        };

        // Act
        var (isSuccess, aggregateId, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(isSuccess);
        Assert.NotEqual(default, aggregateId);
        Assert.Null(error);

        LoggerVerify(LogLevel.Information, ", registered!");
    }
}