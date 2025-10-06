using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.DbContext;

public class TransactionRepository : ITransactionRepository
{
	private readonly DbConnection _dbConnection;
	private readonly DbTransaction _dbTransaction;
	private readonly TransactionFactory _factory;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<TransactionRepository> _logger;

	public TransactionRepository(
		ILogger<TransactionRepository> logger,
		DbConnection dbConnection,
		DbTransaction dbTransaction,
		TransactionFactory factory)
	{
		_logger = logger;
		_dbConnection = dbConnection;
		_dbTransaction = dbTransaction;
		_factory = factory;
	}

	public async Task<IEnumerable<Identity<int>>> GetTransactionsId(
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
		cancellationToken.ThrowIfCancellationRequested();

		return (await Task.Run(
				() =>
					GetTransactionsIdWithTotal(
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
						out _),
				cancellationToken))
			.Select(
				row =>
					new Identity<int>
					{
						Value = row.TransactionId
					});
	}

	public async Task<int> GetTransactionsTotal(
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
		cancellationToken.ThrowIfCancellationRequested();

		return await Task.Run(
			() =>
			{
				GetTransactionsIdWithTotal(
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
					out int pagingTotal);

				return pagingTotal;
			},
			cancellationToken);
	}

	[SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
	private IEnumerable<TransactionIdDto> GetTransactionsIdWithTotal(
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
		out int pagingTotal)
	{
		DynamicParameters parameters = new();

		parameters.Add("@UserId", userId, DbType.Int32);
		parameters.Add("@AccountId", accountId, DbType.Int32);
		parameters.Add("@TransactionType", transactionType, DbType.Boolean);
		parameters.Add("@AmountFrom", amountFrom, DbType.Decimal);
		parameters.Add("@AmountTo", amountTo, DbType.Decimal);
		parameters.Add("@TransactionDateSince", transactionDateSince, DbType.DateTime);
		parameters.Add("@TransactionDateUntil", transactionDateUntil, DbType.DateTime);
		parameters.Add("@ProjectedRepaymentDateSince", projectedRepaymentDateSince, DbType.DateTime);
		parameters.Add("@ProjectedRepaymentDateUntil", projectedRepaymentDateUntil, DbType.DateTime);
		parameters.Add("@PagingSkip", pagingSkip, DbType.Int32);
		parameters.Add("@PagingLimit", pagingLimit, DbType.Int32);
		parameters.Add("@PagingTotal", DbType.Int32, direction: ParameterDirection.Output);

		IEnumerable<TransactionIdDto> result =
			_dbConnection.Query<TransactionIdDto>(
				"usp_SelectTransactions",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction);

		pagingTotal = parameters.Get<int>("@PagingTotal");

		return result;
	}

	[SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
	public async Task<IEnumerable<Identity<int>>> GetTransactionsIdByProjectedRepaymentDate(
		int? accountId,
		DateTime projectedRepaymentDate,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@AccountId", accountId, DbType.Int32);
		parameters.Add("@ProjectedRepaymentDate", projectedRepaymentDate, DbType.DateTime);

		return (await _dbConnection.QueryAsync<TransactionIdDto>(
				"usp_SelectTransactionsOnProjectedRepaymentDate",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					new Identity<int>
					{
						Value = row.TransactionId
					});
	}

	[SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
	public async Task<ITransaction?> GetSingleTransaction(
		int? accountId,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@AccountId", accountId, DbType.Int32);

		return (await _dbConnection.QueryAsync<TransactionDto>(
				"usp_SelectTransaction",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					_factory.Create(
						row.TransactionId,
						row.UserId,
						row.AccountId,
						(TransactionType)row.TransactionType,
						row.Amount,
						row.Description,
						row.TransactionDate,
						row.TransactionFee,
						row.ProjectedRepaymentDate
					))
			.SingleOrDefault();
	}

	[SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
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
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@UserId", userId, DbType.Int32);
		parameters.Add("@AccountId", accountId, DbType.Int32);
		parameters.Add("@TransactionType", transactionType, DbType.Boolean);
		parameters.Add("@Amount", amount, DbType.Decimal);
		parameters.Add("@Description", description, DbType.String);
		parameters.Add("@TransactionDate", transactionDate, DbType.DateTime);
		parameters.Add("@TransactionFee", transactionFee, DbType.Decimal);
		parameters.Add("@ProjectedRepaymentDate", projectedRepaymentDate, DbType.DateTime);

		return (await _dbConnection.QueryAsync<TransactionIdDto>(
				"usp_InsertTransaction",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					new Identity<int>
					{
						Value = row.TransactionId
					})
			.SingleOrDefault();
	}

	[SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands")]
	public async Task<ITransaction?> GetTransaction(
		int transactionId,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@TransactionId", transactionId, DbType.Int32);

		return (await _dbConnection.QueryAsync<TransactionDto>(
				"usp_SelectTransaction",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					_factory.Create(
						row.TransactionId,
						row.UserId,
						row.AccountId,
						(TransactionType)row.TransactionType,
						row.Amount,
						row.Description,
						row.TransactionDate,
						row.TransactionFee,
						row.ProjectedRepaymentDate
					))
			.SingleOrDefault();
	}
}

internal record TransactionIdDto
{
	public int TransactionId { get; init; }
}

internal record TransactionDto
{
	public int TransactionId { get; init; }
	public int? UserId { get; init; }
	public int AccountId { get; init; }
	public int TransactionType { get; init; }
	public decimal Amount { get; init; }
	public string? Description { get; init; }
	public DateTime TransactionDate { get; init; }
	public decimal? TransactionFee { get; init; }
	public DateTime? ProjectedRepaymentDate { get; init; }
}
