using Bc.CashFlow.Domain.User;

namespace Bc.CashFlow.Web.Models.User;

public class UserDetailsViewModel
{
	public UserDetailsViewModel(
		IUser user)
	{
		User = user;
	}

	public IUser User { get; init; }
}