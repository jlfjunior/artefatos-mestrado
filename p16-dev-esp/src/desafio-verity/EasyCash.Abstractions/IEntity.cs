namespace EasyCash.Abstractions;

public interface IEntity<TIdType> : IEntity
{
    TIdType Id { get; }

    void SetId(TIdType id);
}

public interface IEntity
{
    IReadOnlyCollection<DomainEventBase> GetEvents();

    void ClearEvents();
}