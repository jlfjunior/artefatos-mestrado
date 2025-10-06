using System.Data;
using System.Data.Common;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.DbContext;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.IO.DbContext;

public class DailyReportRepository : IDailyReportRepository
{
	private readonly DbConnection _dbConnection;
	private readonly DbTransaction _dbTransaction;
	private readonly DailyReportFactory _factory;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DailyReportRepository> _logger;

	public DailyReportRepository(
		ILogger<DailyReportRepository> logger,
		DbConnection dbConnection,
		DbTransaction dbTransaction,
		DailyReportFactory factory)
	{
		_logger = logger;
		_dbConnection = dbConnection;
		_dbTransaction = dbTransaction;
		_factory = factory;
	}

	public async Task<IEnumerable<Identity<int>>> GetDailyReportsId(
		DateTime? referenceDateSince,
		DateTime? referenceDateUntil,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@ReferenceDateSince", referenceDateSince, DbType.DateTime);
		parameters.Add("@ReferenceDateUntil", referenceDateUntil, DbType.DateTime);

		return (await _dbConnection.QueryAsync<DailyReportIdDto>(
				"usp_SelectDailyReports",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					new Identity<int>
					{
						Value = row.ReportId
					});
	}

	public async Task<Identity<int>?> CreateDailyReport(
		int? accountId,
		DateTime date,
		decimal totalDebits,
		decimal totalCredits,
		decimal totalFee,
		decimal balance,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@AccountId", accountId, DbType.Int32);
		parameters.Add("@ReportDate", date, DbType.DateTime);
		parameters.Add("@TotalDebits", totalDebits, DbType.Int32);
		parameters.Add("@TotalCredits", totalCredits, DbType.Int32);
		parameters.Add("@TotalFee", totalFee, DbType.Int32);
		parameters.Add("@Balance", balance, DbType.Int32);

		return (await _dbConnection.QueryAsync<DailyReportIdDto>(
				"usp_InsertDailyReport",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					new Identity<int>
					{
						Value = row.ReportId
					})
			.SingleOrDefault();
	}

	public async Task<IDailyReport?> GetDailyReport(
		int reportId,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		DynamicParameters parameters = new();

		parameters.Add("@ReportId", reportId, DbType.Int32);

		return (await _dbConnection.QueryAsync<DailyReportDto>(
				"usp_SelectDailyReport",
				parameters,
				commandType: CommandType.StoredProcedure,
				transaction: _dbTransaction))
			.Select(
				row =>
					_factory.Create(
						row.ReportId,
						row.AccountId,
						row.ReportDate,
						row.TotalDebits,
						row.TotalCredits,
						row.TotalFee,
						row.Balance,
						row.CreatedAt
					))
			.SingleOrDefault();
	}
}

file record DailyReportIdDto
{
	public int ReportId { get; init; }
}

file record DailyReportDto
{
	public int ReportId { get; }
	public int? AccountId { get; }
	public DateTime ReportDate { get; }
	public decimal TotalDebits { get; }
	public decimal TotalCredits { get; }
	public decimal TotalFee { get; }
	public decimal Balance { get; }
	public DateTime CreatedAt { get; }
}
