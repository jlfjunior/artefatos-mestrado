using MassTransit;
using Quartz;
using Domain.Models.Consolidation;
using MediatR;
using Application.Launch.Launch.Query.GetAllDailyLaunches;

public class DailyConsolidationJob : IJob
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DailyConsolidationJob> _logger;

    public DailyConsolidationJob(IPublishEndpoint publishEndpoint, IConfiguration configuration, IMediator mediator, ILogger<DailyConsolidationJob> logger)
    {
        _publishEndpoint = publishEndpoint;
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Executing Daily Consolidation...");

        try
        {
            var dailyLaunches = await _mediator.Send(new GetAllPendingDailyLaunchesQuery());

            if (dailyLaunches.Count > 0)
            {
                var batchSize = _configuration.GetValue<string>("LaunchConsolidationBatchSize");

                if (string.IsNullOrEmpty(batchSize))
                {
                    _logger.LogError($"The property LaunchConsolidationBatchSize was not found on config file.");
                    throw new ArgumentException($"The property LaunchConsolidationBatchSize was not found on config file.");
                }

                var batches = BatchHelper.SplitIntoBatches(dailyLaunches, Int32.Parse(batchSize)).ToList();
                int totalBatches = batches.Count;
                int sentBatches = 0;

                _logger.LogInformation($"Total Launches: {dailyLaunches.Count}, Total Batches: {totalBatches}");

                foreach (var batch in batches)
                {
                    var message = new DailyConsolidationEvent { Launches = batch };

                    await _publishEndpoint.Publish(message);
                    sentBatches++;

                    _logger.LogInformation($"Batch {sentBatches}/{totalBatches} sent at {DateTime.UtcNow}, Launches in batch: {batch.Count}");
                }

                _logger.LogInformation("All batches have been sent successfully.");
            }
            else
            {
                _logger.LogInformation("No launches to process.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on sending daily consolidation event.");
        }
    }

}
