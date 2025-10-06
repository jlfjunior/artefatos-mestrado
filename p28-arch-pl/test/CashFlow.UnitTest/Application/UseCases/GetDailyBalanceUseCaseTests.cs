using CashFlow.Application.Queries;
using CashFlow.Application.UseCases;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.UnitTest.Application.UseCases;

public class GetDailyBalanceUseCaseTests : FixtureUseCase<GetDailyBalanceUseCase>
{
    private readonly Mock<ICashFlowRepository> _repository = new();
    private readonly GetDailyBalanceUseCase _useCase;

    public GetDailyBalanceUseCaseTests()
    {
        _useCase = new GetDailyBalanceUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResponseWithCashFlowDetails()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var cashFlow =
            new CashFlowDailyAggregate(Guid.NewGuid(), accountId, DateOnly.FromDateTime(DateTime.UtcNow.Date));
        cashFlow.AddTransaction(new Transaction(cashFlow.Id, 100, TransactionType.Credit));
        cashFlow.AddTransaction(new Transaction(cashFlow.Id, 50, TransactionType.Debit));

        _repository.Setup(repository => repository.GetCurrentCashByAccountId(accountId))
            .ReturnsAsync(cashFlow);

        var command = new GetDailyBalanceQuery
        {
            AccountId = accountId
        };

        // Act
        var (isSuccess, response, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(isSuccess);
        Assert.NotNull(response);
        Assert.Null(error);
        Assert.Equal(cashFlow.GetBalance(), response.CurrentBalance);
        Assert.Equal(cashFlow.Transactions.Count, response.Transactions.Count);

        var transactionDto1 = response.Transactions.First();
        var transactionDto2 = response.Transactions.Last();

        Assert.Equal(cashFlow.Transactions.First().Id, transactionDto1.Id);
        Assert.Equal(cashFlow.Transactions.First().AmountVO.Amount, transactionDto1.AmountVO.Amount);
        Assert.Equal(cashFlow.Transactions.First().Type, transactionDto1.Type);
        Assert.Equal(cashFlow.Transactions.Last().Id, transactionDto2.Id);
        Assert.Equal(cashFlow.Transactions.Last().AmountVO.Amount, transactionDto2.AmountVO.Amount);
        Assert.Equal(cashFlow.Transactions.Last().Type, transactionDto2.Type);

        LoggerVerify(LogLevel.Information, "was found.");
    }


    [Fact]
    public async Task Handle_CashFlowNotFound_ReturnsFailResponse()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();

        _repository.Setup(repository => repository.GetCurrentCashByAccountId(cashFlowId))
            .ReturnsAsync((CashFlowDailyAggregate)null);

        var command = new GetDailyBalanceQuery
        {
            AccountId = cashFlowId
        };

        // Act
        var (isSuccess, response, error) = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(isSuccess);
        Assert.Null(response);
        Assert.Equal(CashFlowNotFound, error!.ErrorCode);

        LoggerVerify(LogLevel.Error, "not found.");
    }
}