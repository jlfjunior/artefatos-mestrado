using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Business;

public class TransactionBusiness : ITransactionBusiness
{
	private readonly IAccountService _accountService;
	private readonly IAccountTypeService _accountTypeService;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<TransactionBusiness> _logger;
	private readonly ITransactionService _transactionService;
	private readonly IUserService _userService;

	public TransactionBusiness(
		ILogger<TransactionBusiness> logger,
		ITransactionService transactionService,
		IAccountService accountService,
		IAccountTypeService accountTypeService,
		IUserService userService)
	{
		_logger = logger;
		_transactionService = transactionService;
		_accountService = accountService;
		_accountTypeService = accountTypeService;
		_userService = userService;
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
		if (transactionType is not null
		    && !Enum.IsDefined(typeof(TransactionType), transactionType))
		{
			throw new TransactionTypeOutOfRangeException();
		}

		return await _transactionService.GetTransactions(
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
	}

	public async Task<ITransaction> GetRequiredTransaction(
		int id,
		CancellationToken cancellationToken)
	{
		ITransaction? result =
			await _transactionService.GetTransaction(
				id,
				cancellationToken);

		if (result is null) throw new TransactionNotFoundException(id);

		return result;
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

	public async Task<Identity<int>> CreateTransaction(
		int? userId,
		int accountId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		CancellationToken cancellationToken)
	{
		IAccount account = await GetRequiredAccount(
			accountId,
			cancellationToken);
		IUser? user = userId is null
			? null
			: await GetRequiredUser(
				userId.Value,
				cancellationToken);
		Identity<int> result = await CreateRequiredTransaction(
			user,
			account,
			transactionType,
			amount,
			description,
			transactionDate,
			cancellationToken);

		await _transactionService.PublishNewTransactionToBalance(
			result.Value,
			cancellationToken);

		return result;
	}

	public async Task UpdateAccountBalance(
		int transactionId,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		ITransaction? transaction =
			await _transactionService.GetTransaction(
				transactionId,
				cancellationToken);

		if (transaction is null) throw new TransactionNotFoundException(transactionId);

		decimal adjustedAmount = transaction.GetAdjustedAmount();

		await _accountService.UpdateBalance(
			transaction.AccountId,
			adjustedAmount,
			cancellationToken);
	}

	public async Task<IAccount> GetRequiredAccount(
		int id,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IAccount? result = await _accountService.GetAccount(
			id,
			cancellationToken);

		if (result is null)
		{
			throw new AccountNotFoundException(id);
		}

		return result;
	}

	public async Task<IAccountType> GetRequiredAccountType(
		IAccount account,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IAccountType? result = await _accountTypeService.GetAccountType(
			account.AccountTypeId,
			cancellationToken);

		if (result is null)
		{
			throw new AccountTypeNotFoundException();
		}

		return result;
	}

	public async Task<IUser> GetRequiredUser(
		int id,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IUser? result = await _userService.GetSingleUser(
			id,
			cancellationToken);

		if (result is null)
		{
			throw new UserNotFoundException(id);
		}

		return result;
	}

	public async Task<Identity<int>> CreateRequiredTransaction(
		IUser? user,
		IAccount account,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IAccountType accountType = await GetRequiredAccountType(
			account,
			cancellationToken);

		decimal transactionFee = GetTransactionFee(
			accountType,
			transactionType,
			amount);
		DateTime projectedRepaymentDate = GetProjectedRepaymentDate(
			accountType,
			transactionDate);

		Identity<int>? result = await _transactionService.CreateTransaction(
			user?.Id,
			account.Id,
			transactionType,
			amount,
			description,
			transactionDate,
			transactionFee,
			projectedRepaymentDate,
			cancellationToken);

		if (result is null)
		{
			throw new TransactionCreationReturnedNullIdentityException();
		}

		return result;
	}

	public static decimal GetTransactionFee(
		IAccountType accountType,
		TransactionType transactionType,
		decimal amount)
	{
		if (!Enum.IsDefined(typeof(TransactionType), transactionType)) throw new TransactionTypeOutOfRangeException();

		if (transactionType is TransactionType.Debit)
		{
			return 0;
		}

		return amount * accountType.BaseFee;
	}

	public static DateTime GetProjectedRepaymentDate(
		IAccountType accountType,
		DateTime transactionDate)
	{
		if (accountType.PaymentDueDays < 0) throw new NegativePaymentDueDaysException();

		return transactionDate.AddDays(accountType.PaymentDueDays);
	}
}
