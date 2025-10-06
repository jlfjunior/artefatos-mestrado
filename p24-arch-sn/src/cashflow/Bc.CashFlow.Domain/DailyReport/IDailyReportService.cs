using Bc.CashFlow.Domain.DbContext;

namespace Bc.CashFlow.Domain.DailyReport;

public interface IDailyReportService
{
	Task<Identity<int>?> CreateDailyReport(
		int? accountId,
		DateTime date,
		decimal totalDebits,
		decimal totalCredits,
		decimal totalFee,
		decimal balance,
		CancellationToken cancellationToken);

	Task<IEnumerable<Identity<int>>> GetDailyReportsId(
		DateTime? referenceDateSince,
		DateTime? referenceDateUntil,
		CancellationToken cancellationToken);

	Task<IDailyReport> GetDailyReport(
		int reportId,
		CancellationToken cancellationToken);
}
