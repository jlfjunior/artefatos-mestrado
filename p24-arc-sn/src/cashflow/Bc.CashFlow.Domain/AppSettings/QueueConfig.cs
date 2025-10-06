using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.AppSettings;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class QueueConfig : IConfig
{
	public string? HostName { get; set; }
	public string? UserName { get; set; }
	public string? Password { get; set; }
	public string? Exchange { get; set; }
	public string? NewTransactionToBalanceQueue { get; set; }
	public string Section => "Queue";
}