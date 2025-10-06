using System.Data;
using System.Data.Common;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.DbContext;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.DbContext;

public class AccountRepository : IAccountRepository
{
	private readonly DbConnection _dbConnection;
	private readonly DbTransaction _dbTransaction;
	private readonly AccountFactory _factory;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AccountRepository> _logger;

	public AccountRepository(
		ILogger<AccountRepository> logger,
		DbConnection dbConnection,
		DbTransaction dbTransaction,
		AccountFactory factory)
	{
		_logger = logger;
		_dbConnection = dbConnection;
		_dbTransaction = dbTransaction;
		_factory = factory;
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

		return (await Task.Run(
				() =>
					GetAccountsIdAndTotal(
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
						out _),
				cancellationToken))
			.Select(
				row =>
					new Identity<int>
					{
						Value = row.AccountId
					});
	}

	public async Task<int> GetAccountsTotal(
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

		return await Task.Run(
			() =>
			{
				GetAccountsIdAndTotal(
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
					out int pagingTotal);

				return pagingTotal;
			},
			cancellationToken);
	}

	private IEnumerable<AccountIdDto> GetAccountsIdAndTotal(
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
		out int pagingTotal)
	{
		DynamicParameters parameters = new();

		parameters.Add("@UserId", userId, DbType.Int32);
		parameters.Add("@AccountTypeId", accountTypeId, DbType.Int32);
		parameters.Add("@AccountName", name, DbType.String);
		parameters.Add("@InitialBalanceFrom", initialBalanceFrom, DbType.Decimal);
		parameters.Add("@InitialBalanceTo", initialBalanceTo, DbType.Decimal);
		parameters.Add("@CurrentBalanceFrom", currentBalanceFrom, DbType.Decimal);
		parameters.Add("@CurrentBalanceTo", currentBalanceTo, DbType.Decimal);
		parameters.Add("@BalanceUpdatedSince", balanceUpdatedAtSince, DbType.DateTime);
		parameters.Add("@BalanceUpdatedUntil", balanceUpdatedAtUntil, DbType.DateTime);
		parameters.Add("@CreatedSince", createdAtSince, DbType.DateTime);
		parameters.Add("@CreatedUntil", createdAtUntil, DbType.Int32);
		parameters.Add("@PagingSkip", pagingSkip, DbType.Int32);
		parameters.Add("@PagingLimit", pagingLimit, DbType.Int32);
		parameters.Add("@PagingTotal", DbType.Int32, direction: ParameterDirection.Output);

		IEnumerable<AccountIdDto> result =
			_dbConnection.Query<AccountIdDto>(
				"usp_SelectAccounts",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction);

		pagingTotal = parameters.Get<int>("@PagingTotal");

		return result;
	}

	public async Task<IAccount?> GetAccount(
		int id,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@AccountId", id, DbType.Int32);

		return (await _dbConnection.QueryAsync<AccountDto>(
				"usp_SelectAccount",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					_factory.Create(
						row.AccountId,
						row.UserId,
						row.AccountTypeId,
						row.AccountName,
						row.InitialBalance,
						row.CurrentBalance,
						row.BalanceUpdatedAt,
						row.CreatedAt
					))
			.SingleOrDefault();
	}

	public async Task UpdateBalance(
		int accountId,
		decimal adjustedAmount,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@AccountId", accountId, DbType.Int32);
		parameters.Add("@AdjustedAmount", adjustedAmount, DbType.Decimal);

		await _dbConnection.ExecuteAsync(
			"usp_UpdateAccountBalance",
			parameters,
			commandType: CommandType.StoredProcedure,
			transaction: _dbTransaction);
	}
}

internal record AccountIdDto
{
	public required int AccountId { get; init; }
}

internal record AccountDto
{
	public required int AccountId { get; init; }
	public required int UserId { get; init; }
	public required int AccountTypeId { get; init; }
	public required string AccountName { get; init; }
	public required decimal InitialBalance { get; init; }
	public required decimal CurrentBalance { get; init; }
	public required DateTime BalanceUpdatedAt { get; init; }
	public required DateTime CreatedAt { get; init; }
}
