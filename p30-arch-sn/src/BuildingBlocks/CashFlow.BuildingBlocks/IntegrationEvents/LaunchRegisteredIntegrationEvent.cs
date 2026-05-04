namespace CashFlow.BuildingBlocks.IntegrationEvents;

public sealed record LaunchRegisteredIntegrationEvent(
    Guid LaunchId,
    decimal Amount,
    string Type,
    DateTime OccurredOnUtc);