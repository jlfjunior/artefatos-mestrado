using EasyCash.Background.Jobs.DailyConsolidation;
using EasyCash.Background.Jobs.IntegrationEventsConsumer;
using EasyCash.Background.Jobs.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace EasyCash.Background.Jobs;

public static class DependencyInjection
{
    public static void AddInfrastructureBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(ops => configuration.GetSection("Outbox").Bind(ops));

        services.AddQuartz(cfg =>
        {
            var scheduler = Guid.NewGuid();
            cfg.SchedulerId = $"job-id-{scheduler}";
            cfg.SchedulerName = $"job-name-{scheduler}";
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<ProcessOutboxMessagesJobSetup>();

        services.AddHostedService<IntegrationEventConsumerJob>();
    }

    public static void AddInfrastructureReportBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(cfg =>
        {
            var scheduler = Guid.NewGuid();
            cfg.SchedulerId = $"job-id-{scheduler}";
            cfg.SchedulerName = $"job-name-{scheduler}";
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<ProcessDailyConsolidationJobSetup>();

        services.AddHostedService<IntegrationEventConsumerJob>();
    }
}
