namespace Bc.CashFlow.Domain.QueueContext;

public class NullQueueHostNameException : Exception
{
	public NullQueueHostNameException() : base("The queue host name cannot be null.")
	{
	}
}