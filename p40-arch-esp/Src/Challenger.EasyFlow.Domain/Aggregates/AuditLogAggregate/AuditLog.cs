using Challenger.EasyFlow.Domain.Common;

namespace Challenger.EasyFlow.Domain.Aggregates.AuditLogAggregate;

public sealed class AuditLog() : Entity<int>(0)
{
    public required string Entity { get; init; }
    public required string Action { get; init; }
    public required string EntityId { get; init; }
    public required string UserId { get; init; }
    public string? ExceptionMessage { get; init; }
}
