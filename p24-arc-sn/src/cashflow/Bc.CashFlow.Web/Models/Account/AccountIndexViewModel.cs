using Bc.CashFlow.Domain.Account;

namespace Bc.CashFlow.Web.Models.Account;

public class AccountIndexViewModel
{
	public AccountIndexViewModel(
		IEnumerable<IAccount> accountsList)
	{
		AccountsList = accountsList;
	}

	public IEnumerable<IAccount> AccountsList { get; init; }
}