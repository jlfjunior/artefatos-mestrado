using EasyCash.Shared.ValueObjects;
using FluentAssertions;

namespace EasyCash.Unit.Tests.Domain.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void FromDecimal_ShouldCreateMoneyWithCorrectAmount()
    {
        // Arrange
        decimal amount = 100m;

        // Act
        Money money = Money.FromDecimal(amount);

        // Assert
        money.Amount.Should().Be(amount);
    }

    [Fact]
    public void ImplicitConversionToDecimal_ShouldReturnCorrectAmount()
    {
        // Arrange
        decimal amount = 100m;
        Money money = Money.FromDecimal(amount);

        // Act
        decimal result = money;

        // Assert
        result.Should().Be(amount);
    }

    [Fact]
    public void AdditionOperator_ShouldReturnCorrectSum()
    {
        // Arrange
        Money money1 = Money.FromDecimal(100m);
        Money money2 = Money.FromDecimal(50m);

        // Act
        Money result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void SubtractionOperator_ShouldReturnCorrectDifference()
    {
        // Arrange
        Money money1 = Money.FromDecimal(100m);
        Money money2 = Money.FromDecimal(50m);

        // Act
        Money result = money1 - money2;

        // Assert
        result.Amount.Should().Be(50m);
    }
}
