using Transaction.Domain.Entity;

namespace Transaction.Application.UseCases.Transaction.Commons;

public class MovementOutput
{
    public string Description { get; private set; } = default!;
    public decimal Value { get; private set; }
    public DateTime Data { get; private set; }

    public MovementOutput(string description, decimal value, DateTime data)
    {
        Description = description;
        Value = value;
        Data = data;
    }

    public static MovementOutput FromDomain(Movement movement)
    {
       return new MovementOutput(movement.Description,movement.Value, movement.Data);
    }
}
