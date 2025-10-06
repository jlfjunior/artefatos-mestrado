namespace Foundation.Business.DomainEvents
{
    public interface IDomainEvent
    {
        IEnumerable<IEvent> Events { get; }
    }
}
