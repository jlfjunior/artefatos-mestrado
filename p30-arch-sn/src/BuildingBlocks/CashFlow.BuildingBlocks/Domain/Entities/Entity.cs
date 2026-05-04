namespace CashFlow.BuildingBlocks.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; private set; }

    protected Entity() {}
    
    protected Entity(Guid id)
    {
        Id = id;
    }
}