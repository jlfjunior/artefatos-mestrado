using Microsoft.Extensions.Options;
using Quartz;

namespace EasyCash.Background.Jobs.DailyConsolidation;

internal sealed class ProcessDailyConsolidationJobSetup : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(DailyConsolidationJob);

        options
            .AddJob<DailyConsolidationJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSchedule(CronScheduleBuilder
                        .DailyAtHourAndMinute(0, 5)));
    }
}
