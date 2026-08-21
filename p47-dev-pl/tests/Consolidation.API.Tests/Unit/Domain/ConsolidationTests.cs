using Xunit;
using FluentAssertions;
using Consolidation.API.Domain.Models;
using Shared.Enums;

namespace Consolidation.API.Tests.Unit.Domain;

public class ConsolidationTests
{
    [Fact]
    public void Create_ShouldReturnConsolidationWithZeroValues()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var consolidation = Consolidation.API.Domain.Models.Consolidation.Create(Lojista.LojaNorte,date);

        // Assert
        consolidation.Should().NotBeNull();
        consolidation.ConsolidationDate.Should().Be(date.Date);
        consolidation.DebitTotal.Should().Be(0);
        consolidation.CreditTotal.Should().Be(0);
        consolidation.DailyBalance.Should().Be(0);
        consolidation.ProcessedCount.Should().Be(0);
    }

    [Fact]
    public void AddTransaction_WithDebit_ShouldIncrementDebitTotal()
    {
        // Arrange
        var consolidation = Consolidation.API.Domain.Models.Consolidation.Create(Lojista.LojaNorte,DateTime.UtcNow);
        var amount = 100.00m;

        // Act
        consolidation.AddTransaction(amount, "Debit");

        // Assert
        consolidation.DebitTotal.Should().Be(100.00m);
        consolidation.CreditTotal.Should().Be(0);
        consolidation.DailyBalance.Should().Be(-100.00m);
        consolidation.ProcessedCount.Should().Be(1);
    }

    [Fact]
    public void AddTransaction_WithCredit_ShouldIncrementCreditTotal()
    {
        // Arrange
        var consolidation = Consolidation.API.Domain.Models.Consolidation.Create(Lojista.LojaNorte,DateTime.UtcNow);
        var amount = 150.50m;

        // Act
        consolidation.AddTransaction(amount, "Credit");

        // Assert
        consolidation.CreditTotal.Should().Be(150.50m);
        consolidation.DebitTotal.Should().Be(0);
        consolidation.DailyBalance.Should().Be(150.50m);
        consolidation.ProcessedCount.Should().Be(1);
    }

    [Fact]
    public void AddTransaction_WithMultipleTransactions_ShouldCalculateCorrectBalance()
    {
        // Arrange
        var consolidation = Consolidation.API.Domain.Models.Consolidation.Create(Lojista.LojaNorte,DateTime.UtcNow);

        // Act
        consolidation.AddTransaction(100m, "Credit");
        consolidation.AddTransaction(50m, "Debit");
        consolidation.AddTransaction(75m, "Credit");

        // Assert
        consolidation.CreditTotal.Should().Be(175m);
        consolidation.DebitTotal.Should().Be(50m);
        consolidation.DailyBalance.Should().Be(125m);
        consolidation.ProcessedCount.Should().Be(3);
    }

    [Fact]
    public void AddTransaction_WithNegativeAmount_ShouldThrow()
    {
        // Arrange
        var consolidation = Consolidation.API.Domain.Models.Consolidation.Create(Lojista.LojaNorte,DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => consolidation.AddTransaction(-50m, "Debit"));
    }

    [Fact]
    public void AddTransaction_WithZeroAmount_ShouldThrow()
    {
        // Arrange
        var consolidation = Consolidation.API.Domain.Models.Consolidation.Create(Lojista.LojaNorte,DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => consolidation.AddTransaction(0m, "Credit"));
    }
}
