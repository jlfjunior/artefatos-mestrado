namespace Bc.CashFlow.Domain.AccountType;

public class AccountTypeNotFoundException : Exception
{
	public AccountTypeNotFoundException() : base("Account type not found.")
	{
	}

	public AccountTypeNotFoundException(int id) : base($"Account type with id `{id}` was not found.")
	{
	}
}
