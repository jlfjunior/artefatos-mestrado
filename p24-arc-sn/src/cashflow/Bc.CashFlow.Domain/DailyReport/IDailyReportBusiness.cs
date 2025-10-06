namespace Bc.CashFlow.Domain.DailyReport;

public interface IDailyReportBusiness
{
	Task ConsolidateDailyReport(
		DateTime referenceDay,
		CancellationToken cancellationToken);

	Task<IEnumerable<IDailyReport>> GetDailyReports(
		DateTime? referenceDateSince,
		DateTime? referenceDateUntil,
		CancellationToken cancellationToken);
}