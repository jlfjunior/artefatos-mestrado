using CashFlow.Application.Queries;
using CashFlow.Application.UseCases;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.UnitTest.Application.UseCases;

public class GetByAccountIdAndDateRangeUseCaseTests : FixtureUseCase<GetByAccountIdAndDateRangeUseCase>
{
    private readonly Mock<ICashFlowRepository> _repository = new();
    private readonly GetByAccountIdAndDateRangeUseCase _useCase;

    public GetByAccountIdAndDateRangeUseCaseTests()
    {
        _useCase = new GetByAccountIdAndDateRangeUseCase(_repository.Object, _logger.Object);
    }

    private GetByAccountIdAndDateRangeQuery _query => new()
    {
        AccountId = Guid.NewGuid()
    };

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var cashFlow =
            new CashFlowDailyAggregate(Guid.NewGuid(), accountId, DateOnly.FromDateTime(DateTime.UtcNow.Date));
        cashFlow.AddTransaction(new Transaction(cashFlow.Id, 100, TransactionType.Credit));
        cashFlow.AddTransaction(new Transaction(cashFlow.Id, 50, TransactionType.Debit));

        _repository.Setup(repository => repository.GetByAccountIdAndDateRange(It.IsAny<Guid>(), It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<CashFlowDailyAggregate?> { cashFlow }, 100, 2));

        // Act
        var (isSuccess, response, error) = await _useCase.Handle(_query, CancellationToken.None);

        // Assert
        Assert.True(isSuccess);
        Assert.NotNull(response);
        Assert.Null(error);
        Assert.NotNull(response.Content);
        Assert.NotNull(response.TotalItems);
        Assert.NotNull(response.TotalPages);

        LoggerVerify(LogLevel.Information, "Account Id:");
    }


    [Fact]
    public async Task Handle_ReturnsFailResponse()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();

        _repository.Setup(repository => repository.GetByAccountIdAndDateRange(It.IsAny<Guid>(), It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<CashFlowDailyAggregate?>(), 0, 0));


        // Act
        var (isSuccess, response, error) = await _useCase.Handle(_query, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Null(response);
        Assert.Equal(CashFlowNotFound, error!.ErrorCode);

        LoggerVerify(LogLevel.Error, "TotalItems");
    }
}