using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.DbContext;
using Microsoft.Extensions.Logging;

namespace Bc.CashFlow.Services;

public class DailyReportService : IDailyReportService
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DailyReportService> _logger;
	private readonly IUnitOfWork _uow;

	public DailyReportService(
		ILogger<DailyReportService> logger,
		IUnitOfWork uow)
	{
		_logger = logger;
		_uow = uow;
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
		return (await _uow.DailyReportRepository.CreateDailyReport(
			accountId,
			date,
			totalDebits,
			totalCredits,
			totalFee,
			balance,
			cancellationToken))!;
	}

	public async Task<IEnumerable<Identity<int>>> GetDailyReportsId(
		DateTime? referenceDateSince,
		DateTime? referenceDateUntil,
		CancellationToken cancellationToken)
	{
		return await _uow.DailyReportRepository.GetDailyReportsId(
			referenceDateSince,
			referenceDateUntil,
			cancellationToken);
	}

	public async Task<IDailyReport> GetDailyReport(
		int reportId,
		CancellationToken cancellationToken)
	{
		IDailyReport? result =
			await _uow.DailyReportRepository.GetDailyReport(
				reportId,
				cancellationToken);

		if (result is null) throw new DailyReportNotFoundException(reportId);

		return result;
	}
}
