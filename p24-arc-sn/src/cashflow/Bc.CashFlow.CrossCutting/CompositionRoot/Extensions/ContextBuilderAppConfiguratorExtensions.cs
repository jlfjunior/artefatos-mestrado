using Microsoft.AspNetCore.Builder;

namespace Bc.CashFlow.CrossCutting.CompositionRoot.Extensions;

public static class ContextBuilderAppConfiguratorExtensions
{
	public static WebApplication ConfigureApp<TStartupContextBuilder>(
		this WebApplication app)
		where TStartupContextBuilder : IContextBuilderAppConfigurator, new()
	{
		return app.Configure<TStartupContextBuilder>();
	}

	private static WebApplication Configure<TStartupContextBuilder>(
		this WebApplication app)
		where TStartupContextBuilder : IContextBuilderAppConfigurator, new()
	{
		return new TStartupContextBuilder().Configure(app);
	}
}