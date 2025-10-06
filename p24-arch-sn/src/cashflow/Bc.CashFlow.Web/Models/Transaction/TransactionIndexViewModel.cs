using Bc.CashFlow.Domain.Transaction;

namespace Bc.CashFlow.Web.Models.Transaction;

public class TransactionIndexViewModel
{
	public TransactionIndexViewModel(
		IEnumerable<ITransaction> transactionsList)
	{
		TransactionsList = transactionsList;
	}

	public IEnumerable<ITransaction> TransactionsList { get; init; }
}