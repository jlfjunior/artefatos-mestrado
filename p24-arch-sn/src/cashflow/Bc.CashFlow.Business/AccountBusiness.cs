using Bc.CashFlow.Domain.Account;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Business;

public class AccountBusiness : IAccountBusiness
{
	private readonly IAccountService _accountService;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AccountBusiness> _logger;

	public AccountBusiness(
		ILogger<AccountBusiness> logger,
		IAccountService accountService)
	{
		_logger = logger;
		_accountService = accountService;
	}

	public async Task<IEnumerable<IAccount>> GetAccounts(
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		return await _accountService.GetAccounts(
			pagingSkip,
			pagingLimit,
			cancellationToken);
	}

	public async Task<IEnumerable<IAccount>> GetAccounts(
		int? userId,
		int? accountTypeId,
		string? name,
		decimal? initialBalanceFrom,
		decimal? initialBalanceTo,
		decimal? currentBalanceFrom,
		decimal? currentBalanceTo,
		DateTime? balanceUpdatedAtSince,
		DateTime? balanceUpdatedAtUntil,
		DateTime? createdAtSince,
		DateTime? createdAtUntil,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		return await _accountService.GetAccounts(
			userId,
			accountTypeId,
			name,
			initialBalanceFrom,
			initialBalanceTo,
			currentBalanceFrom,
			currentBalanceTo,
			balanceUpdatedAtSince,
			balanceUpdatedAtUntil,
			createdAtSince,
			createdAtUntil,
			pagingSkip,
			pagingLimit,
			cancellationToken);
	}

	public async Task<IAccount?> GetAccount(
		int id,
		CancellationToken cancellationToken)
	{
		return await _accountService.GetAccount(
			id,
			cancellationToken);
	}

	public async Task<IAccount> GetRequiredAccount(
		int id,
		CancellationToken cancellationToken)
	{
		IAccount? result =
			await GetAccount(
			id,
			cancellationToken);

		if (result is null) throw new AccountNotFoundException(id);
		
		return result;
	}
}
