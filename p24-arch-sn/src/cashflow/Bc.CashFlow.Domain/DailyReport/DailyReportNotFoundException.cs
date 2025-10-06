namespace Bc.CashFlow.Domain.DailyReport;

public class DailyReportNotFoundException : Exception
{
	public DailyReportNotFoundException() : base("The requested daily report was not found.")
	{
	}

	public DailyReportNotFoundException(int id) : base($"The requested daily report with id `{id}` was not found.")
	{
	}
}
