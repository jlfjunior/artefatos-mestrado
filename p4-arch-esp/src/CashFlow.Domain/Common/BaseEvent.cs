namespace CashFlow.Domain.Common
{
    public abstract class BaseEvent
    {
        public DateTime Timestamp { get; }

        protected BaseEvent()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
