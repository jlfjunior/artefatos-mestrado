namespace Bc.CashFlow.Domain.User;

public interface IUser
{
	int Id { get; }
	string Username { get; }
	DateTime CreatedAt { get; }
}