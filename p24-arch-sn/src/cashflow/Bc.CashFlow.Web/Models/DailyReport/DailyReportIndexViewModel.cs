using Bc.CashFlow.Domain.DailyReport;

namespace Bc.CashFlow.Web.Models.DailyReport;

public class DailyReportIndexViewModel
{
	public DailyReportIndexViewModel(
		IEnumerable<IDailyReport> dailyReportsList)
	{
		DailyReportsList = dailyReportsList;
	}

	public IEnumerable<IDailyReport> DailyReportsList { get; init; }
}