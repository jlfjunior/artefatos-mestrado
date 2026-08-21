using Challenger.EasyFlow.Domain.Aggregates.CashBoxAggregate;

namespace Challenger.EasyFlow.UnitTests.Aggregates;

public class CashBoxTests
{
    [Fact]
    public void Should_Create_CashBox_As_Open_With_Initial_Balance()
    {
        var createdBefore = DateTimeOffset.UtcNow;
        var merchantId = Guid.CreateVersion7();

        var sut = new CashBox(merchantId, "Main", 150.75m);

        var createdAfter = DateTimeOffset.UtcNow;

        Assert.NotEqual(Guid.Empty, sut.Id);
        Assert.Equal(merchantId, sut.MerchantId);
        Assert.Equal("Main", sut.Name);
        Assert.Equal(150.75m, sut.Balance);
        Assert.True(sut.IsOpen);
        Assert.Null(sut.ClosedAt);
        Assert.InRange(sut.OpenedAt, createdBefore, createdAfter);
        Assert.InRange(sut.CreatedAt, createdBefore, createdAfter);
    }
}