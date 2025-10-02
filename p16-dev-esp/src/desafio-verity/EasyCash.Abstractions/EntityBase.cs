namespace EasyCash.Abstractions;

public abstract class EntityBase<TIdType> : IEntity<TIdType>
{
    private readonly List<DomainEventBase> _domainEvents = new();

    public virtual TIdType Id { get; protected set; }

    protected EntityBase(TIdType id)
    {
        Id = id;
    }

    protected EntityBase()
    {
    }

    public virtual void SetId(TIdType id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        Id = id;
    }

    protected abstract void Validate();

    public virtual IReadOnlyCollection<DomainEventBase> GetEvents() => _domainEvents.ToList();

    public virtual void ClearEvents() => _domainEvents.Clear();

    protected virtual void Publish(DomainEventBase domainEvent) => _domainEvents.Add(domainEvent);

    public override bool Equals(object obj) => Equals(obj as EntityBase<TIdType>);

    protected bool Equals(EntityBase<TIdType> other) => other != null && Id.Equals(other.Id);

    public override int GetHashCode() => (object)Id != (object)default(TIdType) ? Id.GetHashCode() : Guid.NewGuid().GetHashCode();

}
