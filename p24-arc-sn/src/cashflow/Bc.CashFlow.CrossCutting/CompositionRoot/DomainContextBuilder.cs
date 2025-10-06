using Bc.CashFlow.CrossCutting.CompositionRoot.Extensions;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.AppSettings;
using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bc.CashFlow.CrossCutting.CompositionRoot;

public class DomainContextBuilder : IContextBuilderInstaller, IContextBuilderConfigBinder
{
	public void BindConfig(
		WebApplicationBuilder builder,
		IConfiguration configuration)
	{
		builder.BindConfig<DatabaseConfig>(configuration);
		builder.BindConfig<CacheConfig>(configuration);
		builder.BindConfig<QueueConfig>(configuration);
		builder.BindConfig<SchedulerConfig>(configuration);
		builder.BindConfig<AntiForgeryConfig>(configuration);
	}

	public void Install(
		WebApplicationBuilder builder,
		IConfiguration? configuration = null)
	{
		builder.Services.AddSingleton<UserFactory>();
		builder.Services.AddSingleton<AccountTypeFactory>();
		builder.Services.AddSingleton<AccountFactory>();
		builder.Services.AddSingleton<TransactionFactory>();
		builder.Services.AddSingleton<DailyReportFactory>();
	}
}
