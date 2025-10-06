using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.AppSettings;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AntiForgeryConfig : IConfig
{
	public string Section => "AntiForgery";

	public string? ApplicationName { get; set; }
	public string? RepositoryType { get; set; }
	public string? RepositoryAddress { get; set; }
}
