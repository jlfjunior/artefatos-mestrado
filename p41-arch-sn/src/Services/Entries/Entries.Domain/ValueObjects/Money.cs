using Entries.Domain.Exceptions;
using Entries.Domain.Primitives;

namespace Entries.Domain.ValueObjects
{
    public sealed class Money : ValueObject
    {
        private static readonly HashSet<string> SupportedCurrencies =
        [
            "BRL", "USD"
        ];

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public decimal Amount { get; }
        public string Currency { get; }

        public static Money Create(decimal amount, string currency)
        {
            if (amount <= 0)
                throw new InvalidAmountException(amount);

            var normalizedCurrency = currency?.Trim().ToUpperInvariant() ?? string.Empty;

            if (!SupportedCurrencies.Contains(normalizedCurrency))
                throw new InvalidCurrencyException(currency ?? string.Empty);

            return new Money(amount, normalizedCurrency);
        }

        public Money Add(Money other)
        {
            if (other.Currency != Currency)
                throw new InvalidOperationException(
                    $"Cannot add amounts in different currencies: {Currency} and {other.Currency}.");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (other.Currency != Currency)
                throw new InvalidOperationException(
                    $"Cannot subtract amounts in different currencies: {Currency} and {other.Currency}.");

            return new Money(Amount - other.Amount, Currency);
        }

        public override string ToString() => $"{Amount:F2} {Currency}";

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
