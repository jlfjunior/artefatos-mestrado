namespace Bc.CashFlow.Domain.User;

public class UserNotFoundException : Exception
{
	public UserNotFoundException() : base("User not found.")
	{
	}
	
	public UserNotFoundException(int id) : base($"User with id `{id}` was not found.")
	{
	}
}
