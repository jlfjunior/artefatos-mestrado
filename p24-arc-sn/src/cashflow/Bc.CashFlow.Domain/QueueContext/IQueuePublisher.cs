namespace Bc.CashFlow.Domain.QueueContext;

public interface IQueuePublisher
{
	Task PublishMessage(
		string message,
		string queue,
		CancellationToken cancellationToken);
}