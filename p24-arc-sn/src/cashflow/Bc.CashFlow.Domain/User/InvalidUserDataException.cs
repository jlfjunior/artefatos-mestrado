namespace Bc.CashFlow.Domain.User;

public class InvalidUserDataException : Exception
{
	// ReSharper disable once UnusedMember.Global
	public InvalidUserDataException() : base("Invalid user data.")
	{
	}

	public InvalidUserDataException(
		string details) : base($"Invalid user data: {details}.")
	{
	}
}