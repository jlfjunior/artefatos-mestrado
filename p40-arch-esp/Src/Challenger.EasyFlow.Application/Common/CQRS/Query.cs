namespace Challenger.EasyFlow.Application.Common.CQRS;

public abstract record Query<TResult>
{
    // CorrelationId can be used for tracing and logging purposes across different components and services.
    public Guid CorrelationId { get; init; } = Guid.CreateVersion7();
}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : Query<TResult>
{
    ValueTask<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}