using Consolidation.Domain.Entities;
using FluentAssertions;

namespace Consolidation.Tests.Domain.Entities
{
    public sealed class DailyBalanceTests
    {
        [Fact]
        public void Create_ValidData_ShouldCreateDailyBalance()
        {
            var dailyBalance = DailyBalance.Create(DateTime.Today, "BRL");

            dailyBalance.Should().NotBeNull();
            dailyBalance.Date.Should().Be(DateTime.Today);
            dailyBalance.Currency.Should().Be("BRL");
            dailyBalance.TotalCredits.Should().Be(0);
            dailyBalance.TotalDebits.Should().Be(0);
            dailyBalance.Balance.Should().Be(0);
        }

        [Fact]
        public void ApplyCredit_ValidAmount_ShouldIncreaseTotalCredits()
        {
            var dailyBalance = DailyBalance.Create(DateTime.Today, "BRL");

            dailyBalance.ApplyCredit(100);

            dailyBalance.TotalCredits.Should().Be(100);
            dailyBalance.Balance.Should().Be(100);
        }

        [Fact]
        public void ApplyDebit_ValidAmount_ShouldIncreaseTotalDebits()
        {
            var dailyBalance = DailyBalance.Create(DateTime.Today, "BRL");

            dailyBalance.ApplyDebit(50);

            dailyBalance.TotalDebits.Should().Be(50);
            dailyBalance.Balance.Should().Be(-50);
        }

        [Fact]
        public void Balance_CreditAndDebit_ShouldReturnCorrectBalance()
        {
            var dailyBalance = DailyBalance.Create(DateTime.Today, "BRL");

            dailyBalance.ApplyCredit(200);
            dailyBalance.ApplyDebit(80);

            dailyBalance.Balance.Should().Be(120);
        }

        [Fact]
        public void ApplyCredit_ZeroAmount_ShouldThrowArgumentException()
        {
            var dailyBalance = DailyBalance.Create(DateTime.Today, "BRL");

            var act = () => dailyBalance.ApplyCredit(0);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ApplyDebit_NegativeAmount_ShouldThrowArgumentException()
        {
            var dailyBalance = DailyBalance.Create(DateTime.Today, "BRL");

            var act = () => dailyBalance.ApplyDebit(-50);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ApplyCredit_MultipleCredits_ShouldAccumulate()
        {
            var dailyBalance = DailyBalance.Create(DateTime.Today, "BRL");

            dailyBalance.ApplyCredit(100);
            dailyBalance.ApplyCredit(200);
            dailyBalance.ApplyCredit(300);

            dailyBalance.TotalCredits.Should().Be(600);
        }
    }
}
