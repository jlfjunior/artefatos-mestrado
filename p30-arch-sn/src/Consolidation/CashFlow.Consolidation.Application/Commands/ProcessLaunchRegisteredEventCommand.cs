using CashFlow.BuildingBlocks.Application.Interfaces;

namespace CashFlow.Consolidation.Application.Commands;

public sealed record ProcessLaunchRegisteredEventCommand(
    Guid LaunchId,
    decimal Amount,
    string Type,
    DateTime OccurredOnUtc) : ICommand;