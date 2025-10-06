namespace Bc.CashFlow.Domain.QueueContext;

public class NullMessageException : Exception
{
	public NullMessageException() : base("Null message exception.")
	{
	}
}
