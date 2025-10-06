using CashFlow.Domain.Exceptions;
using CashFlow.Domain.Services;

namespace CashFlow.UnitTest.Domain.Services;

public class CashFlowServiceTests
{
    private readonly Guid _cashFlowId = Guid.NewGuid();
    private readonly CashFlowService _cashFlowService;

    private readonly CashFlowDailyAggregate _dailyAggregate;
    private readonly Mock<ICashFlowRepository> _mockCashFlowRepository = new();

    public CashFlowServiceTests()
    {
        _dailyAggregate =
            new CashFlowDailyAggregate(_cashFlowId, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.Date));

        _cashFlowService = new CashFlowService(_mockCashFlowRepository.Object);
    }

    [Fact]
    public async Task RegisterNewAggregate_ReturnsSuccess()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _mockCashFlowRepository.Setup(repository => repository.GetCurrentCashByAccountId(It.IsAny<Guid>()))
            .ReturnsAsync(_dailyAggregate);


        // Act
        var aggregate = await _cashFlowService.RegisterNewAggregate(accountId);

        // Assert
        Assert.NotNull(aggregate);
        Assert.NotEqual(default, aggregate.Id);

        _mockCashFlowRepository.Verify(repository => repository.GetCurrentCashByAccountId(accountId), Times.Once);
        _mockCashFlowRepository.Verify(repository => repository.Save(It.IsAny<CashFlowDailyAggregate>()), Times.Once);
    }

    [Fact]
    public async Task AddTransaction_ReturnsTransaction()
    {
        // Arrange
        var amount = 100;
        var type = TransactionType.Debit;

        var cashFlow = _dailyAggregate;
        _mockCashFlowRepository.Setup(repository => repository.GetCurrentCashByAccountId(_cashFlowId))
            .ReturnsAsync(cashFlow);
        _mockCashFlowRepository.Setup(repository => repository.Save(cashFlow))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cashFlowService.AddTransaction(_cashFlowId, amount, type);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_cashFlowId, result.CashFlowId);
        Assert.Equal(amount, result.AmountVO.Amount);
        Assert.Equal(type, result.Type);
        _mockCashFlowRepository.Verify(repository => repository.GetCurrentCashByAccountId(_cashFlowId), Times.Once);
        _mockCashFlowRepository.Verify(repository => repository.Save(cashFlow), Times.Once);
    }

    [Fact]
    public async Task AddTransaction_CashFlowNotFound_ThrowsCashFlowNotFoundException()
    {
        // Arrange
        var amount = 100;
        var type = TransactionType.Debit;

        _mockCashFlowRepository.Setup(repository => repository.GetCurrentCashByAccountId(_cashFlowId))
            .ReturnsAsync((CashFlowDailyAggregate)null);

        // Act
        async Task AddTransaction()
        {
            await _cashFlowService.AddTransaction(_cashFlowId, amount, type);
        }

        // Assert
        await Assert.ThrowsAsync<CashFlowNotFoundException>(AddTransaction);
        _mockCashFlowRepository.Verify(repository => repository.GetCurrentCashByAccountId(_cashFlowId), Times.Once);
        _mockCashFlowRepository.Verify(repository => repository.Save(It.IsAny<CashFlowDailyAggregate>()), Times.Never);
    }

    [Fact]
    public async Task ReverseTransaction_ValidTransaction_ReturnsReversedTransaction()
    {
        // Arrange

        var cashFlow = _dailyAggregate;
        var originalTransaction = new Transaction(_cashFlowId, 100, TransactionType.Debit);
        var transactionId = originalTransaction.Id;

        cashFlow.AddTransaction(originalTransaction);

        _mockCashFlowRepository.Setup(repository => repository.GetByTransactionId(transactionId))
            .ReturnsAsync(cashFlow);
        _mockCashFlowRepository.Setup(repository => repository.Save(cashFlow))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cashFlowService.ReverseTransaction(transactionId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(transactionId, result.Id);
        Assert.Equal(_cashFlowId, result.CashFlowId);
        Assert.Equal(-originalTransaction.AmountVO.Amount, result.AmountVO.Amount);
        Assert.Equal(originalTransaction.Type, result.Type);
        _mockCashFlowRepository.Verify(repository => repository.GetByTransactionId(transactionId), Times.Once);
        _mockCashFlowRepository.Verify(repository => repository.Save(cashFlow), Times.Once);
    }

    [Fact]
    public async Task ReverseTransaction_TransactionNotFound_ThrowsTransactionNotFoundException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _mockCashFlowRepository.Setup(repository => repository.GetByTransactionId(transactionId))
            .ReturnsAsync((CashFlowDailyAggregate)null);

        // Act
        async Task ReverseTransaction()
        {
            await _cashFlowService.ReverseTransaction(transactionId);
        }

        // Assert
        await Assert.ThrowsAsync<TransactionNotFoundException>(ReverseTransaction);
        _mockCashFlowRepository.Verify(repository => repository.GetByTransactionId(transactionId), Times.Once);
        _mockCashFlowRepository.Verify(repository => repository.Save(It.IsAny<CashFlowDailyAggregate>()), Times.Never);
    }
}