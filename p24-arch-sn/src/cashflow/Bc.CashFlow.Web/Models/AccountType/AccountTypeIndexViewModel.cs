using Bc.CashFlow.Domain.AccountType;

namespace Bc.CashFlow.Web.Models.AccountType;

public class AccountTypeIndexViewModel
{
	public AccountTypeIndexViewModel(
		IEnumerable<IAccountType> accountTypesList)
	{
		AccountTypesList = accountTypesList;
	}

	public IEnumerable<IAccountType> AccountTypesList { get; init; }
}