

namespace Foundation.Business.DomainEvents
{
    public abstract record EventBase():IEvent {
        public Guid Id => Guid.NewGuid();
        public string Name => GetType().Name;
        public DateTime Occurred => DateTime.Now;
    }
}