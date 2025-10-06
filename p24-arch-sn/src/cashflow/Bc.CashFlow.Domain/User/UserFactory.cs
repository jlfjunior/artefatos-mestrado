using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.User;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public class UserFactory
{
	public IUser Create(
		int userId,
		string username,
		DateTime createdAt)
	{
		if (userId <= 0)
		{
			throw new InvalidUserDataException("user ID must be grater than 0");
		}

		if (string.IsNullOrWhiteSpace(username))
		{
			throw new InvalidUserDataException("username cannot be null nor white space");
		}

		if (createdAt == DateTime.MinValue)
		{
			throw new InvalidUserDataException("create at cannot be MinDate");
		}

		return new UserVo(
			userId,
			username,
			createdAt
		);
	}
}

file record UserVo(
	int Id,
	string Username,
	DateTime CreatedAt
) : IUser;