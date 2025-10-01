namespace Transaction.Domain.Entity;

public class Movement
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal Value { get; private set; }
    public DateTime Data { get; private set; }

    public Movement(string description, decimal value, DateTime data)
    {
        Id = Guid.NewGuid();
        Description = description;
        Value = value;
        Data = data;
    }
}
