namespace Consolidation.Domain.Entity;

public class Consolidate
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal Value { get; private set; }

    public Consolidate(Guid id, string description, decimal value)
    {
        Id = id;
        Description = description;
        Value = value;
    }
}
