namespace EasyCash.Shared.ValueObjects;

public record Money
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money FromDecimal(decimal amount) => new Money(amount);

    public static implicit operator decimal(Money money) => money.Amount;

    public static Money operator +(Money left, Money right) =>
        new Money(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right) =>
        new Money(left.Amount - right.Amount);
}