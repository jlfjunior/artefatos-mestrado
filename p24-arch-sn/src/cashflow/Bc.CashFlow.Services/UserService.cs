using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.User;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Services;

public class UserService : IUserService
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<UserService> _logger;
	private readonly IUnitOfWork _uow;

	public UserService(
		ILogger<UserService> logger,
		IUnitOfWork uow)
	{
		_logger = logger;
		_uow = uow;
	}

	public async Task<IEnumerable<IUser>> GetUsers(
		string? username,
		DateTime? createdSince,
		DateTime? createdUntil,
		CancellationToken cancellationToken)
	{
		return await _uow.UserRepository.GetUsers(
			username,
			createdSince,
			createdUntil,
			cancellationToken);
	}

	public async Task<IUser?> GetSingleUser(
		int id,
		CancellationToken cancellationToken)
	{
		IUser? result = await _uow.UserRepository.GetSingleUser(
			id,
			cancellationToken);
		
		return result;
	}
}
