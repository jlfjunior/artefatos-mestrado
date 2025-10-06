using Bc.CashFlow.CrossCutting.CompositionRoot.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

// ReSharper disable once InconsistentNaming
public class IOContextBuilder : IContextBuilderInstaller
{
	public void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null)
	{
		builder
			.BuildContext<IODbContextBuilder>(configuration!)
			.BuildContext<IOCacheContextBuilder>(configuration!)
			.BuildContext<IOQueueContextBuilder>(configuration!);
	}
}