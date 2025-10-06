using Bc.CashFlow.Domain.CacheContext;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.QueueContext;
using Bc.CashFlow.Domain.Transaction;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Services;

public class TransactionService : ITransactionService
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<TransactionService> _logger;
	private readonly IQueueContext _q;
	private readonly IUnitOfWork _uow;
	private readonly ICacheContext _cc;

	public TransactionService(
		ILogger<TransactionService> logger,
		IUnitOfWork uow,
		ICacheContext cc,
		IQueueContext q)
	{
		_logger = logger;
		_uow = uow;
		_cc = cc;
		_q = q;
	}

	public async Task<IEnumerable<ITransaction>> GetTransactions(
		int? userId,
		int? accountId,
		TransactionType? transactionType,
		decimal? amountFrom,
		decimal? amountTo,
		DateTime? transactionDateSince,
		DateTime? transactionDateUntil,
		DateTime? projectedRepaymentDateSince,
		DateTime? projectedRepaymentDateUntil,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		IList<ITransaction> result = [];
		IEnumerable<Identity<int>> transactionsIdsList =
			await _uow.TransactionRepository.GetTransactionsId(
				userId,
				accountId,
				transactionType,
				amountFrom,
				amountTo,
				transactionDateSince,
				transactionDateUntil,
				projectedRepaymentDateSince,
				projectedRepaymentDateUntil,
				pagingSkip,
				pagingLimit,
				cancellationToken);

		foreach (Identity<int> identity in transactionsIdsList)
		{
			ITransaction transaction = await GetRequiredTransaction(
				identity.Value,
				cancellationToken);
			
			result.Add(transaction);
		}

		return result;
	}

	private async Task<ITransaction> GetRequiredTransaction(
		int transactionId,
		CancellationToken cancellationToken)
	{
		ITransaction? result =
			await GetTransaction(
				transactionId,
				cancellationToken);

		if (result is null) throw new TransactionNotFoundException(transactionId);

		return result;
	}

	public async Task<IEnumerable<ITransaction>> GetTransactionsOnProjectedRepaymentDate(
		int? accountId,
		DateTime projectedRepaymentDate,
		CancellationToken cancellationToken)
	{
		List<ITransaction> result = [];
		
		IEnumerable<Identity<int>> transactionsIdList =
			await _uow.TransactionRepository.GetTransactionsIdByProjectedRepaymentDate(
				accountId,
				projectedRepaymentDate,
				cancellationToken);

		foreach (Identity<int> identity in transactionsIdList)
		{
			ITransaction transaction = await GetRequiredTransaction(
				identity.Value,
				cancellationToken);

			result.Add(transaction);
		}
		
		return result;
	}

	public async Task<Identity<int>?> CreateTransaction(
		int? userId,
		int accountId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		decimal? transactionFee,
		DateTime? projectedRepaymentDate,
		CancellationToken cancellationToken)
	{
		return await _uow.TransactionRepository.CreateTransaction(
			userId,
			accountId,
			transactionType,
			amount,
			description,
			transactionDate,
			transactionFee,
			projectedRepaymentDate,
			cancellationToken);
	}

	public async Task PublishNewTransactionToBalance(
		int id,
		CancellationToken cancellationToken)
	{
		await _q.PublishNewTransactionToBalance(
			id,
			cancellationToken);
	}

	public async Task<ITransaction?> GetTransaction(
		int id,
		CancellationToken cancellationToken)
	{
		ITransaction? cachedValue =
			await _cc.Transaction.GetValue(
				id.ToString(),
				cancellationToken);

		if (cachedValue is not null)
		{
			_logger.LogDebug("Transaction id {id} retrieved from cache.", id);

			return cachedValue;
		}

		ITransaction? persistedValue =
			await UpdateCache(
				id,
				cancellationToken);

		return persistedValue;
	}

	private async Task<ITransaction?> UpdateCache(
		int id,
		CancellationToken cancellationToken)
	{
		ITransaction? persistedValue =
			await _uow.TransactionRepository.GetTransaction(
				id,
				cancellationToken);

		if (persistedValue is null)
		{
			_logger.LogError("Transaction account with id {id} was not found.", id);

			return null;
		}

		_logger.LogDebug("Transaction id {id} retrieved from database.", id);

		await SetTransactionCache(
			persistedValue,
			cancellationToken);

		return persistedValue;
	}

	private async Task SetTransactionCache(
		ITransaction transaction,
		CancellationToken cancellationToken)
	{
		await _cc.Transaction.SetVale(
			transaction.Id.ToString(),
			transaction,
			cancellationToken);

		_logger.LogDebug("Transaction id {id} added to cache.", transaction.Id);
	}
}
