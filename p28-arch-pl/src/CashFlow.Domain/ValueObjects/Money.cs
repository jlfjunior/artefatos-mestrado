namespace CashFlow.Domain.ValueObjects;

public record Money(decimal Amount)
{
    public Money Add(Money other)
    {
        var newAmount = Amount + other.Amount;
        return new Money(newAmount);
    }

    public Money Subtract(Money other)
    {
        var newAmount = Amount - other.Amount;
        return new Money(newAmount);
    }

    public Money Multiply(decimal multiplier)
    {
        var newAmount = Amount * multiplier;
        return new Money(newAmount);
    }
}