namespace Bc.CashFlow.Domain.QueueContext;

public interface IQueueConsumer
{
	Task QueuePooling(
		string queue,
		Action<string> messageReception,
		CancellationToken cancellationToken);
}
