using MediatR;

namespace Foundation.Business.DomainEvents
{
    public interface IEvent: INotification
    {
        Guid Id { get; }
        string Name { get; }
        DateTime Occurred { get; }        
    }
}
