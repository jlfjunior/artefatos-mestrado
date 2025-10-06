using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Bc.CashFlow.CrossCutting.CompositionRoot.Extensions;

public static class ContextBuilderInstallerExtensions
{
	public static WebApplicationBuilder AddCompositionRoot<TStartupContextBuilder>(
		this WebApplicationBuilder builder)
		where TStartupContextBuilder : IContextBuilderInstaller, new()
	{
		IConfiguration configuration = builder.BuildConfiguration();

		builder
			.BuildContext<DomainContextBuilder>(configuration)
			.BuildContext<IOContextBuilder>(configuration)
			.BuildContext<ServiceContextBuilder>(configuration)
			.BuildContext<BusinessContextBuilder>(configuration)
			.BuildContext<TStartupContextBuilder>(configuration);

		builder.Host.UseDefaultServiceProvider(
			(context, options) =>
			{
				options.ValidateOnBuild = context.HostingEnvironment.IsDevelopment();
				options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
			});

		return builder;
	}

	public static WebApplicationBuilder BuildContext<T>(
		this WebApplicationBuilder builder,
		IConfiguration configuration)
		where T : IContextBuilderInstaller, new()
	{
		T installer = new();

		if (installer is IContextBuilderConfigBinder configurator)
		{
			configurator.BindConfig(
				builder,
				configuration);
		}

		installer.Install(
			builder,
			configuration);

		return builder;
	}

	private static IConfiguration BuildConfiguration(
		this WebApplicationBuilder builder)
	{
		return builder.Configuration
			.AddJsonFile(
				"appsettings.json",
				false,
				true)
			.AddJsonFile(
				$"appsettings.{builder.Environment.EnvironmentName}.json",
				true,
				true)
			.AddEnvironmentVariables()
			.Build();
	}
}
