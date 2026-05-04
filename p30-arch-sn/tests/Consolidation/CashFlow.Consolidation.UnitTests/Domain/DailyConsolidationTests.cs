using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Errors;
using FluentAssertions;

namespace CashFlow.Consolidation.UnitTests.Domain;

public sealed class DailyConsolidationTests
{
    [Fact]
    public void Create_ShouldCalculateBalance_WhenInitialCreditsAndDebitsAreSet()
    {
        // Arrange & Act
        var result = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 100m, 30m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(70m);
    }

    [Fact]
    public void ApplyCredit_ShouldUpdateTotals_WhenAmountIsValid()
    {
        // Arrange
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 100m, 10m).Value;

        // Act
        var result = consolidation.ApplyCredit(25m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        consolidation.TotalCredits.Should().Be(125m);
        consolidation.Balance.Should().Be(115m);
    }

    [Fact]
    public void ApplyDebit_ShouldUpdateTotals_WhenAmountIsValid()
    {
        // Arrange
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 100m, 10m).Value;

        // Act
        var result = consolidation.ApplyDebit(15m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        consolidation.TotalDebits.Should().Be(25m);
        consolidation.Balance.Should().Be(75m);
    }

    [Fact]
    public void ApplyCredit_ShouldFail_WhenAmountIsInvalid()
    {
        // Arrange
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 100m, 10m).Value;

        // Act
        var result = consolidation.ApplyCredit(0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DailyConsolidationErrors.InvalidCreditAmount);
    }

    [Fact]
    public void ApplyDebit_ShouldFail_WhenAmountIsInvalid()
    {
        // Arrange
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 100m, 10m).Value;

        // Act
        var result = consolidation.ApplyDebit(0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DailyConsolidationErrors.InvalidDebitAmount);
    }
}
