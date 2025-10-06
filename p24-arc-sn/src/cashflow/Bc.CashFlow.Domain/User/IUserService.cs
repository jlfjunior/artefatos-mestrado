namespace Bc.CashFlow.Domain.User;

public interface IUserService
{
	Task<IEnumerable<IUser>> GetUsers(
		string? username,
		DateTime? createdSince,
		DateTime? createdUntil,
		CancellationToken cancellationToken);

	Task<IUser?> GetSingleUser(
		int id,
		CancellationToken cancellationToken);
}
