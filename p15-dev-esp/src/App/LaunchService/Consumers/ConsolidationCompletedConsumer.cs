using Application.Launch.Launch.Command.UpdateLaunchConsolidation;
using Domain.Models.Consolidation;
using MassTransit;
using MediatR;

public class ConsolidationCompletedConsumer : IConsumer<ConsolidationCompletedEvent>
{
    private readonly ILogger<ConsolidationCompletedConsumer> _logger;
    private readonly IMediator _mediator;

    public ConsolidationCompletedConsumer(
        IMediator mediator,
        ILogger<ConsolidationCompletedConsumer> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<ConsolidationCompletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Received consolidation response message for {message.Launches.Count} launches");

        var consolidationCommand = new UpdateLaunchConsolidationCommand()
        {
            Launches = message.Launches
        };

        var response = await _mediator.Send(consolidationCommand);
    }
}
