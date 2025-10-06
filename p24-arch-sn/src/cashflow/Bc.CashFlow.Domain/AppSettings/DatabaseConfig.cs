using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.AppSettings;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class DatabaseConfig : IConfig
{
	public string? ConnectionString { get; set; }
	public string Section => "Database";
}