using CashFlow.BuildingBlocks.IntegrationEvents;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Consolidation.Application.Commands;
using MediatR;

namespace CashFlow.Consolidation.Worker.Consumers;

public sealed class LaunchRegisteredConsumer(ISender sender, ILogger<LaunchRegisteredConsumer> logger)
{
    public async Task<Result> ConsumeAsync(
        LaunchRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new ProcessLaunchRegisteredEventCommand(
            integrationEvent.LaunchId,
            integrationEvent.Amount,
            integrationEvent.Type,
            integrationEvent.OccurredOnUtc);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "Failed to process launch registered event. Code: {Code}, Message: {Message}, LaunchId: {LaunchId}",
                result.Error.Code,
                result.Error.Message,
                integrationEvent.LaunchId);

            return result;
        }

        logger.LogInformation(
            "Launch registered event processed successfully. LaunchId: {LaunchId}",
            integrationEvent.LaunchId);
        return Result.Success();
    }
}