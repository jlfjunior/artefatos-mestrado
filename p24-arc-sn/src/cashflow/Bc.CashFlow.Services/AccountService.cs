using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.CacheContext;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Services;

public class AccountService : IAccountService
{
	private readonly ICacheContext _cc;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AccountService> _logger;
	private readonly IUnitOfWork _uow;

	public AccountService(
		ILogger<AccountService> logger,
		IUnitOfWork uow,
		ICacheContext cc)
	{
		_logger = logger;
		_uow = uow;
		_cc = cc;
	}

	public async Task<IEnumerable<IAccount>> GetAccounts(
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		return await GetAccounts(
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
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
		IEnumerable<Identity<int>> accountsIdsList =
			await GetAccountsId(
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
		IEnumerable<IAccount> result =
			await GetAccounts(
				accountsIdsList,
				cancellationToken);

		return result;
	}

	public async Task<IEnumerable<Identity<int>>> GetAccountsId(
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return await _uow.AccountRepository.GetAccountsId(
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			cancellationToken);
	}

	public async Task<IEnumerable<Identity<int>>> GetAccountsId(
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
		cancellationToken.ThrowIfCancellationRequested();

		return await _uow.AccountRepository.GetAccountsId(
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
		IAccount? cachedValue =
			await _cc.Account.GetValue(
				id.ToString(),
				cancellationToken);

		if (cachedValue is not null)
		{
			_logger.LogDebug("Account id {id} retrieved from cache.", id);

			return cachedValue;
		}

		IAccount? persistedValue =
			await UpdateCache(
				id,
				cancellationToken);

		return persistedValue;
	}

	private async Task<IAccount?> UpdateCache(
		int id,
		CancellationToken cancellationToken)
	{
		IAccount? persistedValue =
			await _uow.AccountRepository.GetAccount(
				id,
				cancellationToken);

		if (persistedValue is null)
		{
			_logger.LogError("Expected account with id {id} was not found.", id);

			return null;
		}

		_logger.LogDebug("Account id {id} retrieved from database.", id);

		await SetAccountCache(
			persistedValue,
			cancellationToken);

		return persistedValue;
	}

	private async Task SetAccountCache(
		IAccount account,
		CancellationToken cancellationToken)
	{
		await _cc.Account.SetVale(
			account.Id.ToString(),
			account,
			cancellationToken);

		_logger.LogDebug("Account id {id} added to cache.", account.Id);
	}

	private async Task<IEnumerable<IAccount>> GetAccounts(
		IEnumerable<Identity<int>> accountsIdsList,
		CancellationToken cancellationToken)
	{
		List<IAccount> result = [];

		foreach (Identity<int> identity in accountsIdsList)
		{
			IAccount? account =
				await GetAccount(
					identity.Value,
					cancellationToken);

			if (account is null)
			{
				continue;
			}

			result.Add(account);
		}

		return result;
	}

	public async Task UpdateBalance(
		int accountId,
		decimal adjustedAmount,
		CancellationToken cancellationToken)
	{
		IAccount? account =
			await GetAccount(
				accountId,
				cancellationToken);

		if (account is null) throw new AccountNotFoundException();

		await _uow.AccountRepository.UpdateBalance(
			accountId,
			adjustedAmount,
			cancellationToken);

		await UpdateCache(
			accountId,
			cancellationToken);
	}
}
