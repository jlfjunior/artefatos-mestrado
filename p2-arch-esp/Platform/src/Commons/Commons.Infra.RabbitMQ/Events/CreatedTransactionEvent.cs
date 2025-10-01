namespace Commons.Infra.RabbitMQ.Events;

public class CreatedTransactionEvent
{
    public Guid TransactionId { get; set; }
    public Guid MovementId { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal Value { get; private set; }
    public DateTime Data { get; private set; }
    public DateTime CreatedAt { get; set; }

    public CreatedTransactionEvent(Guid transactionId, Guid movementId, string description, decimal value, DateTime data, DateTime createdAt)
    {
        TransactionId = transactionId;
        MovementId = movementId;
        Description = description;
        Value = value;
        Data = data;
        CreatedAt = createdAt;
    }
}
