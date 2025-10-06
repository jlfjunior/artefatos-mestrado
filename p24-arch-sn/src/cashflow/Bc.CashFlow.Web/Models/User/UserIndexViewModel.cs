using Bc.CashFlow.Domain.User;

namespace Bc.CashFlow.Web.Models.User;

public class UserIndexViewModel
{
	public UserIndexViewModel(
		IEnumerable<IUser> usersList)
	{
		UsersList = usersList;
	}

	public IEnumerable<IUser> UsersList { get; init; }
}