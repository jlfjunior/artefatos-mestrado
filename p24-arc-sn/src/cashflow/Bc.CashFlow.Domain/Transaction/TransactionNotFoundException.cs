namespace Bc.CashFlow.Domain.Transaction;

public class TransactionNotFoundException : Exception
{
	public TransactionNotFoundException() : base("Transaction not found.")
	{
	}

	public TransactionNotFoundException(int id) : base($"Transaction with id `{id}` was not found.")
	{
	}
}
