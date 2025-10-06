namespace Bc.CashFlow.Domain.Transaction;

public static class TransactionExtensions
{
	public static decimal GetAdjustedAmount(
		this ITransaction transaction)
	{
		return transaction.TransactionType
			switch
			{
				TransactionType.Credit => transaction.Amount - (transaction.TransactionFee ?? 0),
				TransactionType.Debit => transaction.Amount * -1,
				_ => throw new TransactionTypeOutOfRangeException()
			};
	}
}
