using CashFlow.BuildingBlocks.Results;

namespace CashFlow.Launches.Domain.Errors;

public static class LaunchErrors
{
    public static readonly Error InvalidAmount =
        new("launch.invalid_amount", "The launch amount must be greater than zero.", ErrorType.Business);

    public static readonly Error InvalidType =
        new("launch.invalid_type", "The launch type is invalid.", ErrorType.Business);
}