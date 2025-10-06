namespace Bc.CashFlow.Domain.QueueContext;

public interface IQueueContext
{
	Task PublishNewTransactionToBalance(
		int id,
		CancellationToken cancellationToken);

	Task IterateNewTransactionToBalanceQueue(Action<TransactionIdMessage?> messageReception,
		CancellationToken cancellationToken);
}
