using Bc.CashFlow.Domain.DailyReport;

namespace Bc.CashFlow.Scheduler.Jobs;

public class DailyReportJob : IJob
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DailyReportJob> _logger;
	private readonly IServiceProvider _serviceProvider;

	public DailyReportJob(
		ILogger<DailyReportJob> logger,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	public void Run()
	{
		// ReSharper disable once ConvertToUsingDeclaration
		using (IServiceScope scope = _serviceProvider.CreateScope())
		{
			IDailyReportBusiness dailyReportBusiness = scope.ServiceProvider
				.GetRequiredService<IDailyReportBusiness>();

			dailyReportBusiness.ConsolidateDailyReport(
					DateTime.Today.AddDays(-1).Date,
					CancellationToken.None)
				.GetAwaiter()
				.GetResult();
		}
	}
}
