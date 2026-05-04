using CashFlow.BuildingBlocks.Domain.Entities;

namespace CashFlow.Consolidation.Domain.Entities;

public sealed class ProcessedLaunchEvent : Entity
{
    public Guid LaunchId { get; private set; }
    public DateTime ProcessedOnUtc { get; private set; }

    private ProcessedLaunchEvent() : base(Guid.Empty)
    {
    }

    private ProcessedLaunchEvent(Guid id, Guid launchId, DateTime processedOnUtc) : base(id)
    {
        LaunchId = launchId;
        ProcessedOnUtc = processedOnUtc;
    }

    public static ProcessedLaunchEvent Create(Guid launchId, DateTime processedOnUtc)
    {
        return new ProcessedLaunchEvent(Guid.NewGuid(), launchId, processedOnUtc);
    }
}
