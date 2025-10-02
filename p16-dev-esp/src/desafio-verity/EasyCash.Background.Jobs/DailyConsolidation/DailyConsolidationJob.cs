using EasyCash.Abstractions.Interfaces;
using EasyCash.Report.Command.Consolidations.DailyConsolidations;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EasyCash.Background.Jobs.DailyConsolidation;

[DisallowConcurrentExecution]
internal sealed class DailyConsolidationJob(IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<DailyConsolidationJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var date = DateOnly.FromDateTime(dateTimeProvider.UtcNow.AddDays(-1));
        logger.LogInformation("Beginning to process daily consolidation for date {date}", date);

        var command = new DailyConsolidationCommand()
        {
            Date = date
        };

        try
        {
            await publisher.Publish(command, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Exception while processing daily consolidation for date {date}",
                date);
        }
        finally
        {
            logger.LogInformation("Completed processing daily consolidation for date {date}", date);
        }
    }
}

