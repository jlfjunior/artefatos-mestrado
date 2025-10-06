namespace Bc.CashFlow.Domain.User;

public interface IUserBusiness
{
	Task<IEnumerable<IUser>> GetUsers(
		string? username,
		DateTime? createdSince,
		DateTime? createdUntil,
		CancellationToken cancellationToken);

	Task<IUser> GetRequiredUser(
		int id,
		CancellationToken cancellationToken);
}
