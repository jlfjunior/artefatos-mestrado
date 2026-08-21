namespace Challenger.EasyFlow.Domain.Common;

public abstract class Entity<TId>(TId id)
{
    public TId Id { get; private init; } = id;
    public DateTimeOffset CreatedAt { get; private init; } = DateTimeOffset.UtcNow;
}