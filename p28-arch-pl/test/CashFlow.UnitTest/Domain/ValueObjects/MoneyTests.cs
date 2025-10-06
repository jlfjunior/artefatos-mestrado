using CashFlow.Domain.ValueObjects;

namespace CashFlow.UnitTest.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Add_ReturnsCorrectResult()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(50);

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(150, result.Amount);
    }

    [Fact]
    public void Add_ReturnsFailResult()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(50);

        // Act
        try
        {
            var result = money1.Add(money2);
        }
        catch (InvalidOperationException ex)
        {
            Assert.Equal("Cannot add money with different currencies.", ex.Message);
        }
    }

    [Fact]
    public void Subtract_ReturnsCorrectResult()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(50);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(50, result.Amount);
    }

    [Fact]
    public void Subtract_ReturnsFailResult()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(50);

        // Act
        try
        {
            var result = money1.Subtract(money2);
        }
        catch (InvalidOperationException ex)
        {
            Assert.Equal("Cannot subtract money with different currencies.", ex.Message);
        }
    }

    [Fact]
    public void Multiply_ReturnsCorrectResult()
    {
        // Arrange
        var money = new Money(100);
        var multiplier = 2;

        // Act
        var result = money.Multiply(multiplier);

        // Assert
        Assert.Equal(200, result.Amount);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualObjects()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(100);

        // Act
        var result = money1.Equals(money2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentObjects()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(200);

        // Act
        var result = money1.Equals(money2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEquals_ReturnsTrueForEqualObjects()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(100);

        // Act
        var result = money1 == money2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEquals_ReturnsTrueForDifferentObjects()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(200);

        // Act
        var result = money1 != money2;

        // Assert
        Assert.True(result);
    }
}