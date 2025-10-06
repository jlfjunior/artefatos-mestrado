namespace Bc.CashFlow.Domain.User;

// ReSharper disable once InconsistentNaming
public interface IUserRepository
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