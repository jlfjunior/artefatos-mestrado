using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;

namespace Challenger.EasyFlow.UnitTests.Aggregates;

public class TransactionTests
{
    [Fact]
    public void Should_Create_Transaction_With_Expected_Values()
    {
        var cashBoxId = Guid.CreateVersion7();

        var sut = new Transaction
        {
            CashBoxId = cashBoxId,
            Type = TransactionType.Credit,
            Amount = 99.9m,
            Category = TransactionCategory.Sale,
            PaymentMethod = PaymentMethod.Pix,
            Description = "Order #10"
        };

        Assert.NotEqual(Guid.Empty, sut.Id);
        Assert.Equal(cashBoxId, sut.CashBoxId);
        Assert.Equal(TransactionType.Credit, sut.Type);
        Assert.Equal(99.9m, sut.Amount);
        Assert.Equal(TransactionCategory.Sale, sut.Category);
        Assert.Equal(PaymentMethod.Pix, sut.PaymentMethod);
        Assert.Equal("Order #10", sut.Description);
    }
}