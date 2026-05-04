using CashFlow.BuildingBlocks.Domain.Entities;
using CashFlow.BuildingBlocks.Results;
using CashFlow.Launches.Domain.Enums;
using CashFlow.Launches.Domain.Errors;

namespace CashFlow.Launches.Domain.Entities;

public sealed class Launch : Entity
{
    public decimal Amount { get; private set; }
    public LaunchType Type { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    
    private Launch() : base(Guid.Empty)
    {
    }

    private Launch(Guid id, decimal amount, LaunchType type, DateTime occurredOnUtc)
        : base(id)
    {
        Amount = amount;
        Type = type;
        OccurredOnUtc = occurredOnUtc;
    }

    public static Result<Launch> Create(decimal amount, LaunchType type, DateTime occurredOnUtc)
    {
        if (amount <= 0)
            return Result<Launch>.Failure(LaunchErrors.InvalidAmount);

        if (!Enum.IsDefined(type))
            return Result<Launch>.Failure(LaunchErrors.InvalidType);

        var launch = new Launch(
            Guid.NewGuid(),
            amount,
            type,
            occurredOnUtc);

        return Result<Launch>.Success(launch);
    }
}