using Bc.CashFlow.Domain.User;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Business;

public class UserBusiness : IUserBusiness
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<UserBusiness> _logger;
	private readonly IUserService _userService;

	public UserBusiness(
		ILogger<UserBusiness> logger,
		IUserService userService)
	{
		_logger = logger;
		_userService = userService;
	}

	public async Task<IEnumerable<IUser>> GetUsers(
		string? username,
		DateTime? createdSince,
		DateTime? createdUntil,
		CancellationToken cancellationToken)
	{
		return await _userService.GetUsers(
			username,
			createdSince,
			createdUntil,
			cancellationToken);
	}

	public async Task<IUser> GetRequiredUser(
		int id,
		CancellationToken cancellationToken)
	{
		IUser? result = await _userService.GetSingleUser(
			id,
			cancellationToken);

		if (result is null) throw new UserNotFoundException(id);
		
		return result;
	}
}
