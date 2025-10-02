namespace EasyCash.Abstractions.Messaging.Commands;

public abstract class IdempotencyCommandBase<TCommandResult> : ICommand<TCommandResult>, IIdempotency
    where TCommandResult : ICommandResult
{
    public string IdempotencyKey { get; set; }
}

public interface IIdempotency
{
    public string IdempotencyKey { get; set; }
}