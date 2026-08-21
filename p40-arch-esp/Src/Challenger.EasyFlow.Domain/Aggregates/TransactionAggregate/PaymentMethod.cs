namespace Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;

public sealed record PaymentMethod(int Id, string Name)
{
    public readonly static PaymentMethod Cash = new(1, "Cash");
    public readonly static PaymentMethod CreditCard = new(2, "Credit Card");
    public readonly static PaymentMethod DebitCard = new(3, "Debit Card");
    public readonly static PaymentMethod Pix = new(4, "Pix");

    public static IEnumerable<PaymentMethod> AsList() => [Cash, CreditCard, DebitCard, Pix];

    public static PaymentMethod FromId(int to)
        => AsList().Single(x => x.Id == to);
}