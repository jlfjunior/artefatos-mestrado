using CashFlow.BuildingBlocks.Application.Interfaces;

namespace CashFlow.Launches.Application.Commands;

public sealed record RegisterLaunchCommand(
    decimal Amount,
    string Type,
    DateTime OccurredOnUtc) : ICommand<Guid>;