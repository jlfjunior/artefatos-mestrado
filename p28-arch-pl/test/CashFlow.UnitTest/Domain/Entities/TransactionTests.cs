using CashFlow.Domain.ValueObjects;

namespace CashFlow.UnitTest.Domain.Entities;

public class TransactionTests
{
    [Fact]
    public void Merge_ValidTransaction_ReturnsMergedTransaction()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var originalTransaction = new Transaction(cashFlowId, 100, TransactionType.Credit);
        var otherTransaction = new Transaction(cashFlowId, 50, TransactionType.Credit);

        // Act
        var mergedTransaction = originalTransaction.Merge(otherTransaction);

        // Assert
        Assert.NotNull(mergedTransaction);
        Assert.Equal(cashFlowId, mergedTransaction.CashFlowId);
        Assert.Equal(150, mergedTransaction.AmountVO.Amount);
        Assert.Equal(TransactionType.Credit, mergedTransaction.Type);
    }

    [Fact]
    public void Reverse_WithPositiveAmount_ReturnsTransactionWithNegativeAmount()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var reversedTransaction = transaction.Reverse();

        // Assert
        Assert.Equal(-amount.Amount, reversedTransaction.AmountVO.Amount);
        Assert.Equal(type, reversedTransaction.Type);
        Assert.Equal(cashFlowId, reversedTransaction.CashFlowId);
        Assert.NotEqual(transaction.Id, reversedTransaction.Id);
    }

    [Fact]
    public void IsPositiveAmount_WithPositiveAmount_ReturnsTrue()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var isPositiveAmount = transaction.IsPositiveAmount();

        // Assert
        Assert.True(isPositiveAmount);
    }

    [Fact]
    public void IsPositiveAmount_WithNegativeAmount_ReturnsFalse()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(-100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var isPositiveAmount = transaction.IsPositiveAmount();

        // Assert
        Assert.False(isPositiveAmount);
    }

    [Fact]
    public void IsNegativeAmount_WithNegativeAmount_ReturnsTrue()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(-100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var isNegativeAmount = transaction.IsNegativeAmount();

        // Assert
        Assert.True(isNegativeAmount);
    }

    [Fact]
    public void IsNegativeAmount_WithPositiveAmount_ReturnsFalse()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var isNegativeAmount = transaction.IsNegativeAmount();

        // Assert
        Assert.False(isNegativeAmount);
    }

    [Fact]
    public void ApplyPercentage_ValidPercentage_ReturnsNewTransactionWithAdjustedAmount()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var originalTransaction = new Transaction(cashFlowId, 100, TransactionType.Credit);
        var percentage = 10;

        // Act
        var adjustedTransaction = originalTransaction.ApplyPercentage(percentage);

        // Assert
        Assert.NotNull(adjustedTransaction);
        Assert.Equal(cashFlowId, adjustedTransaction.CashFlowId);
        Assert.Equal(10, adjustedTransaction.AmountVO.Amount);
        Assert.Equal(TransactionType.Credit, adjustedTransaction.Type);
    }

    [Fact]
    public void ApplyPercentage_ReturnsTransactionWithCorrectAmount()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);
        var percentage = 50;
        var expectedAmount = amount.Amount * (percentage / 100m);

        // Act
        var newTransaction = transaction.ApplyPercentage(percentage);

        // Assert
        Assert.Equal(expectedAmount, newTransaction.AmountVO.Amount);
        Assert.Equal(type, newTransaction.Type);
        Assert.Equal(cashFlowId, newTransaction.CashFlowId);
    }

    [Fact]
    public void WithAmount_ReturnsNewTransactionWithUpdatedAmount()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);
        var newAmount = 200;

        // Act
        var updatedTransaction = transaction.WithAmount(newAmount);

        // Assert
        Assert.Equal(newAmount, updatedTransaction.AmountVO.Amount);
        Assert.Equal(type, updatedTransaction.Type);
        Assert.Equal(cashFlowId, updatedTransaction.CashFlowId);
        Assert.NotEqual(transaction.Id, updatedTransaction.Id);
    }

    [Fact]
    public void WithTimestamp_ReturnsNewTransactionWithUpdatedTimestamp()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);
        var newTimestamp = new DateTime(2023, 1, 1);

        // Act
        var updatedTransaction = transaction.WithTimestamp(newTimestamp);

        // Assert
        Assert.Equal(amount.Amount, updatedTransaction.AmountVO.Amount);
        Assert.Equal(type, updatedTransaction.Type);
        Assert.Equal(cashFlowId, updatedTransaction.CashFlowId);
        Assert.NotEqual(transaction.Id, updatedTransaction.Id);
    }

    [Fact]
    public void GetFormattedAmount_ReturnsFormattedAmountString()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100.50m);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var formattedAmount = transaction.GetFormattedAmount();

        // Assert
        Assert.Equal("100.50", formattedAmount);
    }

    [Fact]
    public void IsSameTransaction_WithDifferentTransaction_ReturnsFalse()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction1 = new Transaction(cashFlowId, amount.Amount, type);
        var transaction2 = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var isSameTransaction = transaction1.IsSameTransaction(transaction2);

        // Assert
        Assert.False(isSameTransaction);
    }

    [Fact]
    public void IsSameTransaction_WithSameTransaction_ReturnsTrue()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var isSameTransaction = transaction.IsSameTransaction(transaction);

        // Assert
        Assert.True(isSameTransaction);
    }

    [Fact]
    public void IsInFuture_WithFutureTimestamp_ReturnsTrue()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var futureTimestamp = DateTime.UtcNow.AddMinutes(10);
        var transaction = new Transaction(cashFlowId, amount.Amount, type).WithTimestamp(futureTimestamp);

        // Act
        var isInFuture = transaction.IsInFuture();

        // Assert
        Assert.True(isInFuture);
    }

    [Fact]
    public void IsSameAmount_WithDifferentTransaction_ReturnsFalse()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount1 = new Money(100);
        var amount2 = new Money(200);
        var type = TransactionType.Debit;
        var transaction1 = new Transaction(cashFlowId, amount1.Amount, type);
        var transaction2 = new Transaction(cashFlowId, amount2.Amount, type);

        // Act
        var isSameAmount = transaction1.IsSameAmount(transaction2);

        // Assert
        Assert.False(isSameAmount);
    }

    [Fact]
    public void IsSameAmount_WithSameTransaction_ReturnsTrue()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var transaction = new Transaction(cashFlowId, amount.Amount, type);

        // Act
        var isSameAmount = transaction.IsSameAmount(transaction);

        // Assert
        Assert.True(isSameAmount);
    }

    [Fact]
    public void HasSameTimestamp_WithDifferentTransaction_ReturnsFalse()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var timestamp1 = new DateTime(2023, 1, 1);
        var timestamp2 = new DateTime(2023, 2, 1);
        var transaction1 = new Transaction(cashFlowId, amount.Amount, type).WithTimestamp(timestamp1);
        var transaction2 = new Transaction(cashFlowId, amount.Amount, type).WithTimestamp(timestamp2);

        // Act
        var hasSameTimestamp = transaction1.HasSameTimestamp(transaction2);

        // Assert
        Assert.False(hasSameTimestamp);
    }

    [Fact]
    public void HasSameTimestamp_WithSameTransaction_ReturnsTrue()
    {
        // Arrange
        var cashFlowId = Guid.NewGuid();
        var amount = new Money(100);
        var type = TransactionType.Debit;
        var timestamp = new DateTime(2023, 1, 1);
        var transaction = new Transaction(cashFlowId, amount.Amount, type).WithTimestamp(timestamp);

        // Act
        var hasSameTimestamp = transaction.HasSameTimestamp(transaction);

        // Assert
        Assert.True(hasSameTimestamp);
    }
}