namespace Challenger.EasyFlow.Domain.Common;

public interface IUnitOfWork
{
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
    ValueTask UseTransactionAsync(Func<ValueTask> action, CancellationToken cancellationToken = default);
}