using CashFlow.BuildingBlocks.IntegrationEvents;

namespace CashFlow.Launches.Application.Interfaces;

public interface ILaunchIntegrationEventPublisher
{
    Task PublishRegisteredAsync( LaunchRegisteredIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
