using CashFlow.Domain.ValueObjects;

namespace CashFlow.UnitTest.Domain.Aggregates;

public class CashFlowAggregateTests
{
    private readonly CashFlowDailyAggregate _dailyAggregate = new(Guid.NewGuid(), Guid.NewGuid(),
        DateOnly.FromDateTime(DateTime.UtcNow.Date));

    [Fact]
    public void AddTransaction_ValidTransaction_AddsTransactionAndEvent()
    {
        // Arrange
        var amount = new Money(100);
        var transactionType = TransactionType.Credit;

        var transaction = new Transaction(_dailyAggregate.Id, amount.Amount, transactionType);

        // Act
        _dailyAggregate.AddTransaction(transaction);

        // Assert
        Assert.Single(_dailyAggregate.Transactions);
        Assert.Equal(transaction, _dailyAggregate.Transactions.First());
    }

    [Fact]
    public void GetBalance_WithTransactions_ReturnsCorrectBalance()
    {
        // Arrange

        var transactions = new List<Transaction>
        {
            new(_dailyAggregate.Id, 100, TransactionType.Credit),
            new(_dailyAggregate.Id, 50, TransactionType.Debit)
        };

        transactions.ForEach(t => _dailyAggregate.AddTransaction(t));

        // Act
        var balance = _dailyAggregate.GetBalance();

        // Assert
        Assert.Equal(50, balance);
    }

    [Fact]
    public void GetTransactionsInRange_WithTransactions_ReturnsCorrectTransactions()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);


        var transaction = new Transaction(_dailyAggregate.Id, 100, TransactionType.Credit);
        var transaction1 = new Transaction(_dailyAggregate.Id, 50, TransactionType.Debit);
        var transaction2 = new Transaction(_dailyAggregate.Id, 75, TransactionType.Credit);

        var transactions = new List<Transaction>
        {
            transaction.WithTimestamp(new DateTime(2023, 2, 15)),
            transaction1.WithTimestamp(new DateTime(2022, 6, 30)),
            transaction2.WithTimestamp(new DateTime(2023, 12, 1))
        };

        transactions.ForEach(t => _dailyAggregate.AddTransaction(t));

        // Act
        var result = _dailyAggregate.GetTransactionsInRange(startDate, endDate);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(transactions[0], result[0]);
        Assert.Equal(transactions[2], result[1]);
    }

    [Fact]
    public void GetTotalDebits_WithTransactions_ReturnsCorrectTotalDebits()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(_dailyAggregate.Id, 100, TransactionType.Credit),
            new(_dailyAggregate.Id, 50, TransactionType.Debit),
            new(_dailyAggregate.Id, 75, TransactionType.Debit)
        };


        transactions.ForEach(t => _dailyAggregate.AddTransaction(t));

        // Act
        var totalDebits = _dailyAggregate.GetTotalDebits();

        // Assert
        Assert.Equal(125, totalDebits);
    }

    [Fact]
    public void GetTRansactionsByType_WithTransactions_ReturnsCorrectTotalDebits()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(_dailyAggregate.Id, 100, TransactionType.Credit),
            new(_dailyAggregate.Id, 50, TransactionType.Debit),
            new(_dailyAggregate.Id, 75, TransactionType.Debit)
        };

        transactions.ForEach(t => _dailyAggregate.AddTransaction(t));

        // Act
        var totalDebits = _dailyAggregate.GetTransactionsByType(TransactionType.Debit);

        // Assert
        Assert.Equal(2, totalDebits.Count);
    }

    [Fact]
    public void GetTotalCredits_WithTransactions_ReturnsCorrectTotalCredits()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(_dailyAggregate.Id, 100, TransactionType.Credit),
            new(_dailyAggregate.Id, 50, TransactionType.Credit),
            new(_dailyAggregate.Id, 75, TransactionType.Debit)
        };


        transactions.ForEach(t => _dailyAggregate.AddTransaction(t));

        // Act
        var totalCredits = _dailyAggregate.GetTotalCredits();

        // Assert
        Assert.Equal(150, totalCredits);
    }

    [Fact]
    public void ReverseTransaction_WithExistingTransaction_RemovesTransaction()
    {
        // Arrange
        var amount = new Money(100);
        var transactionType = TransactionType.Credit;

        var transaction = new Transaction(_dailyAggregate.Id, amount.Amount, transactionType);

        _dailyAggregate.AddTransaction(transaction);

        // Act
        _dailyAggregate.ReverseTransaction(transaction.Id);

        // Assert
        Assert.Equal(2, _dailyAggregate.Transactions.Count);
        Assert.Equal(0, _dailyAggregate.GetBalance());
    }

    [Fact]
    public void ReverseTransaction_FindNull_RemovesTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var amount = new Money(100);
        var transactionType = TransactionType.Credit;

        var transaction = new Transaction(_dailyAggregate.Id, amount.Amount, transactionType)
            ;

        _dailyAggregate.AddTransaction(transaction);

        // Act
        var transactionReverted = _dailyAggregate.ReverseTransaction(transactionId);

        // Assert
        Assert.Null(transactionReverted);
        Assert.Equal(1, _dailyAggregate.Transactions.Count);
    }

    [Fact]
    public void HasNegativeBalance_WithPositiveBalance_ReturnsFalse()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(_dailyAggregate.Id, 100, TransactionType.Credit),
            new(_dailyAggregate.Id, 50, TransactionType.Debit)
        };


        transactions.ForEach(t => _dailyAggregate.AddTransaction(t));

        // Act
        var hasNegativeBalance = _dailyAggregate.HasNegativeBalance();

        // Assert
        Assert.False(hasNegativeBalance);
    }

    [Fact]
    public void HasNegativeBalance_WithNegativeBalance_ReturnsTrue()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(_dailyAggregate.Id, 50, TransactionType.Debit),
            new(_dailyAggregate.Id, 100, TransactionType.Debit)
        };

        transactions.ForEach(t => _dailyAggregate.AddTransaction(t));

        // Act
        var hasNegativeBalance = _dailyAggregate.HasNegativeBalance();

        // Assert
        Assert.True(hasNegativeBalance);
    }
}