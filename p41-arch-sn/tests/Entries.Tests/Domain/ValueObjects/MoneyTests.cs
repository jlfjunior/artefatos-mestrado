using Entries.Domain.Exceptions;
using Entries.Domain.ValueObjects;
using FluentAssertions;

namespace Entries.Tests.Domain.ValueObjects
{
    public sealed class MoneyTests
    {
        [Fact]
        public void Create_ValidAmountAndCurrency_ShouldCreateMoney()
        {
            var money = Money.Create(100, "BRL");

            money.Amount.Should().Be(100);
            money.Currency.Should().Be("BRL");
        }

        [Fact]
        public void Create_ZeroAmount_ShouldThrowInvalidAmountException()
        {
            var act = () => Money.Create(0, "BRL");

            act.Should().Throw<InvalidAmountException>();
        }

        [Fact]
        public void Create_NegativeAmount_ShouldThrowInvalidAmountException()
        {
            var act = () => Money.Create(-100, "BRL");

            act.Should().Throw<InvalidAmountException>();
        }

        [Fact]
        public void Create_InvalidCurrency_ShouldThrowInvalidCurrencyException()
        {
            var act = () => Money.Create(100, "XYZ");

            act.Should().Throw<InvalidCurrencyException>();
        }

        [Fact]
        public void Create_LowercaseCurrency_ShouldNormalizeToCurrency()
        {
            var money = Money.Create(100, "brl");

            money.Currency.Should().Be("BRL");
        }

        [Fact]
        public void Add_SameCurrency_ShouldReturnCorrectSum()
        {
            var money1 = Money.Create(100, "BRL");
            var money2 = Money.Create(50, "BRL");

            var result = money1.Add(money2);

            result.Amount.Should().Be(150);
            result.Currency.Should().Be("BRL");
        }

        [Fact]
        public void Add_DifferentCurrencies_ShouldThrowInvalidOperationException()
        {
            var money1 = Money.Create(100, "BRL");
            var money2 = Money.Create(50, "USD");

            var act = () => money1.Add(money2);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Equals_SameAmountAndCurrency_ShouldBeEqual()
        {
            var money1 = Money.Create(100, "BRL");
            var money2 = Money.Create(100, "BRL");

            money1.Should().Be(money2);
        }

        [Fact]
        public void Equals_DifferentAmount_ShouldNotBeEqual()
        {
            var money1 = Money.Create(100, "BRL");
            var money2 = Money.Create(200, "BRL");

            money1.Should().NotBe(money2);
        }
    }
}
