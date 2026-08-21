using CashFlow.Domain.Entities;
using FluentAssertions;

namespace CashFlow.Domain.Tests;

public class DailyBalanceTests
{
    [Fact]
    public void Empty_ShouldInitializeZeroedBalance()
    {
        var userId = Guid.NewGuid();
        var date = new DateOnly(2026, 6, 13);

        var balance = DailyBalance.Empty(userId, date);

        balance.UserId.Should().Be(userId);
        balance.BalanceDate.Should().Be(date);
        balance.TotalCredits.Should().Be(0);
        balance.TotalDebits.Should().Be(0);
        balance.Balance.Should().Be(0);
    }
}
