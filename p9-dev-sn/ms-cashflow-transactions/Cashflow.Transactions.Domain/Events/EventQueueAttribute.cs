namespace Cashflow.Transactions.Domain.Events
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EventQueueAttribute : Attribute
    {
        public string Name { get; }

        public EventQueueAttribute(string name)
        {
            Name = name;
        }
    }
}
