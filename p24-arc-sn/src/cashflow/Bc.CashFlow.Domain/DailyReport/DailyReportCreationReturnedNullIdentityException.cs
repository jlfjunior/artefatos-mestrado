namespace Bc.CashFlow.Domain.DailyReport;

public class DailyReportCreationReturnedNullIdentityException : Exception
{
	public DailyReportCreationReturnedNullIdentityException() : base("Daily report creation returned null identity.")
	{
	}
}