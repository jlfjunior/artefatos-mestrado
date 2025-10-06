namespace Bc.CashFlow.Domain.Transaction;

public class TransactionCreationReturnedNullIdentityException : Exception
{
	public TransactionCreationReturnedNullIdentityException() : base(
		"A transaction creation returned null identity value.")
	{
	}
}