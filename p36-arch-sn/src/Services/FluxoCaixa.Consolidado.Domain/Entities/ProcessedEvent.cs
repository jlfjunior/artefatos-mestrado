namespace FluxoCaixa.Consolidado.Domain.Entities;

public class ProcessedEvent
{
    public Guid EventId { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }

    protected ProcessedEvent() { }

    public ProcessedEvent(Guid eventId)
    {
        EventId = eventId;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
