using Bc.CashFlow.Domain.Account;

namespace Bc.CashFlow.Web.Models.Account;

public class AccountDetailsViewModel
{
	public AccountDetailsViewModel(
		IAccount account)
	{
		Account = account;
	}

	public IAccount Account { get; init; }
}
