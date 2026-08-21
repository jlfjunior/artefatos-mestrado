using Challenger.EasyFlow.Domain.Aggregates.AuditLogAggregate;

namespace Challenger.EasyFlow.UnitTests.Aggregates;

public class AuditLogTests
{
    [Fact]
    public void Should_Create_AuditLog_With_Expected_Values()
    {
        var createdBefore = DateTimeOffset.UtcNow;

        var sut = new AuditLog
        {
            Entity = "CashBox",
            Action = "Create",
            EntityId = "entity-1",
            UserId = "user-1",
            ExceptionMessage = null
        };

        var createdAfter = DateTimeOffset.UtcNow;

        Assert.Equal(0, sut.Id);
        Assert.Equal("CashBox", sut.Entity);
        Assert.Equal("Create", sut.Action);
        Assert.Equal("entity-1", sut.EntityId);
        Assert.Equal("user-1", sut.UserId);
        Assert.Null(sut.ExceptionMessage);
        Assert.InRange(sut.CreatedAt, createdBefore, createdAfter);
    }
}