using CashFlow.Domain.Entities;
using CashFlow.Shared.Authorization;
using CashFlow.Shared.Enums;
using CashFlow.Domain.Services;
using FluentAssertions;

namespace CashFlow.Domain.Tests;

public class TransactionTests
{
    [Fact]
    public void Create_ShouldInitializeTransaction_WhenDataIsValid()
    {
        var transaction = Transaction.Create(
            "Venda",
            100m,
            EntryType.Credit,
            new DateOnly(2026, 6, 13),
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId);

        transaction.Amount.Should().Be(100m);
        transaction.EntryType.Should().Be(EntryType.Credit);
        transaction.SignedAmount().Should().Be(100m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_ShouldThrow_WhenAmountIsInvalid(decimal amount)
    {
        var act = () => Transaction.Create("Test", amount, EntryType.Debit, DateOnly.FromDateTime(DateTime.UtcNow), SeedIdentifiers.ClientUserId, SeedIdentifiers.EmployeeUserId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SignedAmount_ShouldBeNegative_ForDebit()
    {
        var transaction = Transaction.Create(
            "Aluguel",
            50m,
            EntryType.Debit,
            new DateOnly(2026, 6, 13),
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId);

        transaction.SignedAmount().Should().Be(-50m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenDescriptionIsInvalid(string description)
    {
        var act = () => Transaction.Create(
            description,
            10m,
            EntryType.Credit,
            DateOnly.FromDateTime(DateTime.UtcNow),
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var act = () => Transaction.Create(
            "Test",
            10m,
            EntryType.Credit,
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.Empty,
            SeedIdentifiers.EmployeeUserId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Restore_ShouldRehydrateTransaction()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var transaction = Transaction.Restore(
            id,
            "Restaurado",
            42m,
            EntryType.Credit,
            new DateOnly(2026, 1, 1),
            SeedIdentifiers.ClientUserId,
            createdAt,
            SeedIdentifiers.EmployeeUserId);

        transaction.Id.Should().Be(id);
        transaction.Description.Should().Be("Restaurado");
        transaction.CreatedAt.Should().Be(createdAt);
    }
}

public class DailyBalanceCalculatorTests
{
    [Fact]
    public void ApplyDelta_ShouldRecalculateBalance()
    {
        var result = DailyBalanceCalculator.ApplyDelta(100, 20, 50, 10);

        result.TotalCredits.Should().Be(150);
        result.TotalDebits.Should().Be(30);
        result.Balance.Should().Be(120);
    }

    [Fact]
    public void ComputeAdditionDelta_ShouldSplitCreditAndDebit()
    {
        var credit = DailyBalanceCalculator.ComputeAdditionDelta(80m, EntryType.Credit);
        credit.CreditDelta.Should().Be(80m);
        credit.DebitDelta.Should().Be(0);

        var debit = DailyBalanceCalculator.ComputeAdditionDelta(30m, EntryType.Debit);
        debit.CreditDelta.Should().Be(0);
        debit.DebitDelta.Should().Be(30m);
    }

    [Fact]
    public void ComputeCumulativeBalance_ShouldIncludePreviousBalance()
    {
        DailyBalanceCalculator.ComputeCumulativeBalance(150m, 50m, 10m).Should().Be(190m);
        DailyBalanceCalculator.ComputeCumulativeBalance(0m, 0m, 0m).Should().Be(0m);
    }
}
