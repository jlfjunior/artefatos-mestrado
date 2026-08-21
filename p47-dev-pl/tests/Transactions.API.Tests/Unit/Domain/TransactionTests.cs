using Xunit;
using FluentAssertions;
using Shared.Enums;
using Transactions.API.Domain.Models;

namespace Transactions.API.Tests.Unit.Domain;

public class TransactionTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnTransaction()
    {
        // Arrange
        var amount = 100.50m;
        var type = TransactionType.Credit;
        var description = "Test transaction";
        var date = DateTime.UtcNow;

        // Act
        var transaction = Transaction.Create(Lojista.LojaNorte,amount, type, description, date);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Amount.Should().Be(amount);
        transaction.Type.Should().Be(type);
        transaction.Description.Should().Be(description);
        transaction.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = -50.00m;
        var type = TransactionType.Debit;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Transaction.Create(Lojista.LojaNorte,amount, type, null, DateTime.UtcNow));
    }

    [Fact]
    public void Create_WithFutureDate_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = 100.00m;
        var type = TransactionType.Credit;
        var futureDate = DateTime.UtcNow.AddDays(2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Transaction.Create(Lojista.LojaNorte,amount, type, null, futureDate));
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetIsProcessedTrue()
    {
        // Arrange
        var transaction = Transaction.Create(Lojista.LojaNorte,100m, TransactionType.Credit, null, DateTime.UtcNow);
        transaction.IsProcessed.Should().BeFalse();

        // Act
        transaction.MarkAsProcessed();

        // Assert
        transaction.IsProcessed.Should().BeTrue();
    }

    [Theory]
    [InlineData(TransactionType.Debit)]
    [InlineData(TransactionType.Credit)]
    public void Create_WithDifferentTypes_ShouldSucceed(TransactionType type)
    {
        // Act
        var transaction = Transaction.Create(Lojista.LojaNorte,50m, type, "Test", DateTime.UtcNow);

        // Assert
        transaction.Type.Should().Be(type);
    }
}
