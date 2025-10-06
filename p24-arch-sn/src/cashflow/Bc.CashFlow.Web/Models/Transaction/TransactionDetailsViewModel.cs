using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;

namespace Bc.CashFlow.Web.Models.Transaction;

public class TransactionDetailsViewModel
{
	public TransactionDetailsViewModel(
		ITransaction transaction,
		IUser? user)
	{
		Transaction = transaction;
		User = user;
	}
	
	public ITransaction Transaction { get; }
	public IUser User { get; }
}
