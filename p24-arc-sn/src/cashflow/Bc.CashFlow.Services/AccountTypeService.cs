using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.CacheContext;
using Bc.CashFlow.Domain.DbContext;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Services;

public class AccountTypeService : IAccountTypeService
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AccountTypeService> _logger;
	private readonly IUnitOfWork _uow;
	private readonly ICacheContext _cc;

	public AccountTypeService(
		ILogger<AccountTypeService> logger,
		IUnitOfWork uow,
		ICacheContext cc)
	{
		_logger = logger;
		_uow = uow;
		_cc = cc;
	}

	public async Task<IEnumerable<IAccountType>> GetAccountTypes(
		string? name,
		decimal? baseFeeFrom,
		decimal? baseFeeTo,
		int? paymentDueDaysFrom,
		int? paymentDueDaysTo,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		List<IAccountType> result = [];

		IEnumerable<Identity<int>> accountTypesIdList =
			await _uow.AccountTypeRepository.GetAccountTypesId(
				name,
				baseFeeFrom,
				baseFeeTo,
				paymentDueDaysFrom,
				paymentDueDaysTo,
				pagingSkip,
				pagingLimit,
				cancellationToken);

		foreach (Identity<int> identity in accountTypesIdList)
		{
			IAccountType accountType =
				await GetRequiredAccountType(
					identity.Value,
					cancellationToken);

			result.Add(accountType);
		}

		return result;
	}

	public async Task<IAccountType?> GetAccountType(
		int id,
		CancellationToken cancellationToken)
	{
		
		IAccountType? cachedValue =
			await _cc.AccountType.GetValue(
				id.ToString(),
				cancellationToken);

		if (cachedValue is not null)
		{
			_logger.LogDebug("Account type id {id} retrieved from cache.", id);

			return cachedValue;
		}

		IAccountType? persistedValue =
			await UpdateCache(
				id,
				cancellationToken);

		return persistedValue;
	}

	private async Task<IAccountType?> UpdateCache(
		int id,
		CancellationToken cancellationToken)
	{
		IAccountType? persistedValue =
			await _uow.AccountTypeRepository.GetAccountType(
				id,
				cancellationToken);

		if (persistedValue is null)
		{
			_logger.LogError("Account type account with id {id} was not found.", id);

			return null;
		}

		_logger.LogDebug("Account type id {id} retrieved from database.", id);

		await SetAccountType(
			persistedValue,
			cancellationToken);

		return persistedValue;
	}

	private async Task SetAccountType(
		IAccountType accountType,
		CancellationToken cancellationToken)
	{
		await _cc.AccountType.SetVale(
			accountType.Id.ToString(),
			accountType,
			cancellationToken);

		_logger.LogDebug("Account type id {id} added to cache.", accountType.Id);
	}

	private async Task<IAccountType> GetRequiredAccountType(
		int id,
		CancellationToken cancellationToken)
	{
		IAccountType? result =
			await GetAccountType(
				id,
				cancellationToken);

		if (result is null) throw new AccountTypeNotFoundException(id);

		return result;
	}
}
