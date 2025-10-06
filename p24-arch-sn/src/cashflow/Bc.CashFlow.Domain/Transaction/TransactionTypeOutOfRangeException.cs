namespace Bc.CashFlow.Domain.Transaction;

public class TransactionTypeOutOfRangeException : OutOfMemoryException
{
	public TransactionTypeOutOfRangeException() : base("Transaction type out of range.")
	{
	}
}