namespace Bc.CashFlow.Domain.Account;

public class AccountNotFoundException : Exception
{
	public AccountNotFoundException() : base("Account not found.")
	{
	}

	public AccountNotFoundException(int id) : base($"Account with id `{id}` was not found.")
	{
	}
}
