using Bc.CashFlow.Domain.AppSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bc.CashFlow.CrossCutting.CompositionRoot.Extensions;

public static class ContextBuilderConfigBinderExtensions
{
	// ReSharper disable once UnusedMethodReturnValue.Global
	public static WebApplicationBuilder BindConfig<T>(
		this WebApplicationBuilder builder,
		IConfiguration configuration)
		where T : class, IConfig, new()
	{
		T configurator = new();

		configuration.GetSection(configurator.Section)
			.Bind(configurator);

		builder.Services.AddSingleton(configurator);

		return builder;
	}
}