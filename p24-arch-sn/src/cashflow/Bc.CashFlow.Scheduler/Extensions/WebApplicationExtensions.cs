using System.Linq.Expressions;
using Bc.CashFlow.CrossCutting.CompositionRoot;
using Bc.CashFlow.CrossCutting.CompositionRoot.Extensions;
using Bc.CashFlow.Scheduler.Jobs;
using Hangfire;

namespace Bc.CashFlow.Scheduler.Extensions;

public static class WebApplicationExtensions
{
	public static WebApplicationBuilder Setup(
		this WebApplicationBuilder builder)
	{
		builder.AddCompositionRoot<SchedulerContextBuilder>();
		
		builder.Services.AddSingleton<DailyReportJob>();
		builder.Services.AddSingleton<BalanceNewAccountTransactionJob>();

		return builder;
	}

	public static WebApplication Configure(
		this WebApplication app)
	{
		return ((WebApplication)app
				.UseHangfireDashboard())
			.ConfigureApp<SchedulerContextBuilder>()
			.AddOrUpdate<DailyReportJob>(
				"daily-report",
				"0 0 * * *")
			.AddOrUpdate<BalanceNewAccountTransactionJob>(
				"balance-new-account-transaction",
				"*/5 * * * *");
	}

	// ReSharper disable once UnusedMember.Local
	private static WebApplication EnqueueJob<TJob>(
		this WebApplication app)
		where TJob : IJob
	{
		return app.EnqueueJob(
			() =>
				app.Services.GetRequiredService<TJob>()
					.Run());
	}

	private static WebApplication EnqueueJob(
		this WebApplication app,
		Expression<Action> methodCall)
	{
		IBackgroundJobClient backgroundJobs = app.Services
			.GetRequiredService<IBackgroundJobClient>();

		backgroundJobs.Enqueue(methodCall);

		return app;
	}

	// ReSharper disable once UnusedMember.Local
	private static WebApplication AddOrUpdate<TJob>(
		this WebApplication app,
		string jobName,
		string cronExpression)
		where TJob : IJob
	{
		return app.AddOrUpdate(
			jobName,
			() =>
				app.Services.GetRequiredService<TJob>().Run(),
			cronExpression);
	}

	private static WebApplication AddOrUpdate(
		this WebApplication app,
		string jobName,
		Expression<Action> methodCall,
		string cronExpression)
	{
		IRecurringJobManager recurringJobs = app.Services
			.GetRequiredService<IRecurringJobManager>();

		recurringJobs.AddOrUpdate(
			jobName,
			methodCall,
			cronExpression);

		return app;
	}
}
