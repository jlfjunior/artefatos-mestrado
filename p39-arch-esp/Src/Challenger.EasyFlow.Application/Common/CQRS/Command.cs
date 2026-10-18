using System.Text.Json.Serialization;

namespace Challenger.EasyFlow.Application.Common.CQRS;

public abstract record Command
{
    // Metadata
    // CorrelationId can be used for tracing and logging purposes across different components and services.
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public Dictionary<string, object> Metadata { get; init; } = [];
}
public abstract record Command<TResult> : Command;

public interface ICommandHandler<TCommand>
    where TCommand : Command
{
    ValueTask HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : Command<TResult>
{
    ValueTask<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
