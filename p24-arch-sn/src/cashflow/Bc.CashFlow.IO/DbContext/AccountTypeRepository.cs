using System.Data;
using System.Data.Common;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.DbContext;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.DbContext;

public class AccountTypeRepository : IAccountTypeRepository
{
	private readonly DbConnection _dbConnection;
	private readonly DbTransaction _dbTransaction;
	private readonly AccountTypeFactory _factory;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AccountTypeRepository> _logger;

	public AccountTypeRepository(
		ILogger<AccountTypeRepository> logger,
		DbConnection dbConnection,
		DbTransaction dbTransaction,
		AccountTypeFactory factory)
	{
		_logger = logger;
		_dbConnection = dbConnection;
		_dbTransaction = dbTransaction;
		_factory = factory;
	}

	public async Task<IEnumerable<Identity<int>>> GetAccountTypesId(
		string? name,
		decimal? baseFeeFrom,
		decimal? baseFeeTo,
		int? paymentDueDaysFrom,
		int? paymentDueDaysTo,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return (await Task.Run(
				() =>
					GetAccountTypesIdWithTotal(
						name,
						baseFeeFrom,
						baseFeeTo,
						paymentDueDaysFrom,
						paymentDueDaysTo,
						pagingSkip,
						pagingLimit,
						out _),
				cancellationToken))
			.Select(
				row =>
					new Identity<int> {
						Value = row.AccountTypeId,
					});
	}

	public async Task<int> GetAccountTypesTotal(
		string? name,
		decimal? baseFeeFrom,
		decimal? baseFeeTo,
		int? paymentDueDaysFrom,
		int? paymentDueDaysTo,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return (await Task.Run(
				() =>
				{
					GetAccountTypesIdWithTotal(
						name,
						baseFeeFrom,
						baseFeeTo,
						paymentDueDaysFrom,
						paymentDueDaysTo,
						pagingSkip,
						pagingLimit,
						out int pagingTotal);

					return pagingTotal;
				},
				cancellationToken));
	}

	private IEnumerable<AccountTypeIdDto> GetAccountTypesIdWithTotal(
		string? name,
		decimal? baseFeeFrom,
		decimal? baseFeeTo,
		int? paymentDueDaysFrom,
		int? paymentDueDaysTo,
		int? pagingSkip,
		int? pagingLimit,
		out int pagingTotal)
	{
		DynamicParameters parameters = new();

		parameters.Add("@AccountTypeName", name, DbType.String);
		parameters.Add("@BaseFeeFrom", baseFeeFrom, DbType.Decimal);
		parameters.Add("@BaseFeeTo", baseFeeTo, DbType.Decimal);
		parameters.Add("@PaymentDueDaysFrom", paymentDueDaysFrom, DbType.Int32);
		parameters.Add("@PaymentDueDaysTo", paymentDueDaysTo, DbType.Int32);
		parameters.Add("@PagingSkip", pagingSkip, DbType.Int32);
		parameters.Add("@PagingLimit", pagingLimit, DbType.Int32);
		parameters.Add("@PagingTotal", DbType.Int32, direction: ParameterDirection.Output);

		IEnumerable<AccountTypeIdDto> result =
			_dbConnection.Query<AccountTypeIdDto>(
				"usp_SelectAccountTypes",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction);

		pagingTotal = parameters.Get<int>("@PagingTotal");
		
		return result;
	}

	public async Task<IAccountType?> GetAccountType(
		int id,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@AccountTypeId", id, DbType.Int32);

		return (await _dbConnection.QueryAsync<AccountTypeDto>(
				"usp_SelectAccountType",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					_factory.Create(
						row.AccountTypeId,
						row.AccountTypeName,
						row.BaseFee,
						row.PaymentDueDays
					))
			.SingleOrDefault();
	}
}

internal record AccountTypeIdDto
{
	public required int AccountTypeId { get; init; }
}

internal record AccountTypeDto
{
	public required int AccountTypeId { get; init; }
	public required string AccountTypeName { get; init; }
	public required decimal BaseFee { get; init; }
	public required int PaymentDueDays { get; init; }
}
