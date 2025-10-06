using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.AppSettings;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SchedulerConfig : IConfig
{
	public HangfireConfig? Hangfire { get; set; }
	public string Section => "Scheduler";

	public class HangfireConfig
	{
		public string? ConnectionString { get; set; }
	}
}