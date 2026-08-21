using Challenger.EasyFlow.Domain.Aggregates.DailyBalanceAggregate;

namespace Challenger.EasyFlow.UnitTests.Aggregates;

public class DailyBalanceTests
{
    [Fact]
    public void Should_Create_DailyBalance_With_Expected_Values()
    {
        var cashBoxId = Guid.CreateVersion7();

        var sut = new DailyBalance
        {
            CashBoxId = cashBoxId,
            OpeningBalance = 100m,
            ClosingBalance = 130m,
            TotalInflow = 50m,
            TotalOutflow = 20m
        };

        Assert.NotEqual(Guid.Empty, sut.Id);
        Assert.Equal(cashBoxId, sut.CashBoxId);
        Assert.Equal(100m, sut.OpeningBalance);
        Assert.Equal(130m, sut.ClosingBalance);
        Assert.Equal(50m, sut.TotalInflow);
        Assert.Equal(20m, sut.TotalOutflow);
    }
}