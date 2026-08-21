using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;

namespace Challenger.EasyFlow.UnitTests.Aggregates;

public class PaymentMethodTests
{
    [Fact]
    public void AsList_Should_Return_All_Known_Methods()
    {
        var list = PaymentMethod.AsList().ToList();

        Assert.Equal(4, list.Count);
        Assert.Contains(PaymentMethod.Cash, list);
        Assert.Contains(PaymentMethod.CreditCard, list);
        Assert.Contains(PaymentMethod.DebitCard, list);
        Assert.Contains(PaymentMethod.Pix, list);
    }

    [Fact]
    public void FromId_Should_Return_Method_When_Id_Exists()
    {
        var method = PaymentMethod.FromId(4);

        Assert.Equal(PaymentMethod.Pix, method);
    }

    [Fact]
    public void FromId_Should_Throw_When_Id_Does_Not_Exist()
    {
        Assert.Throws<InvalidOperationException>(() => PaymentMethod.FromId(999));
    }
}