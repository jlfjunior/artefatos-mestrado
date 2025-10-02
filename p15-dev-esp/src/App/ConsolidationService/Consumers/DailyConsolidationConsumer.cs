using Application.Consolidation.Consolidation.Command.CreateConsolidation;
using Domain.Models.Consolidation;
using MassTransit;
using MediatR;

public class DailyConsolidationConsumer : IConsumer<DailyConsolidationEvent>
{
    private readonly ILogger<DailyConsolidationConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;

    public DailyConsolidationConsumer(
        IMediator mediator,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<DailyConsolidationConsumer> logger)
    {
        _logger = logger;
        _mediator = mediator;
        _configuration = configuration;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<DailyConsolidationEvent> context)
    {
        var message = context.Message;
        _logger.LogDebug($"Received daily consolidation message for {message.Launches.Count} launches.");

        if (message.Launches.Count > 0)
        {
            var batchSize = _configuration.GetValue<string>("LaunchConsolidationBatchSize");

            if (string.IsNullOrEmpty(batchSize))
            {
                _logger.LogError($"The property LaunchConsolidationBatchSize was not found on config file.");
                throw new ArgumentException($"The property LaunchConsolidationBatchSize was not found on config file.");
            }

            var batches = BatchHelper.SplitIntoBatches(message.Launches, Int32.Parse(batchSize)).ToList();
            int totalBatches = batches.Count;
            int processedBatches = 0;

            _logger.LogDebug($"Total launches: {message.Launches.Count}, Total Batches: {totalBatches}");

            foreach (var batch in batches)
            {
                var consolidationCommand = new CreateConsolidationCommand
                {
                    Launches = batch
                };

                var launches = await _mediator.Send(consolidationCommand);
                processedBatches++;

                var consolidationCompletedEvent = new ConsolidationCompletedEvent
                {
                    Launches = launches
                };

                await _publishEndpoint.Publish(consolidationCompletedEvent);

                _logger.LogDebug($"Batch {processedBatches}/{totalBatches} processed. Launches in batch: {batch.Count}");
            }

            _logger.LogDebug("All consolidation batches processed successfully.");
        }
    }

}
