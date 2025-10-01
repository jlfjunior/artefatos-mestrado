namespace Consolidation.Application.UseCases.Consolidation.Commons;

public class ConsolidateOutput
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal Value { get; private set; }

    public ConsolidateOutput(Guid id, string description, decimal value)
    {
        Id = id;
        Description = description;
        Value = value;
    }
}
