using System.Reflection;

namespace Cashflow.Management.Workers.Events
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

    public static class EventQueueAttributeExtensions
    {
        public static string GetEventQueue<T>()
        {
            var type = typeof(T);
            var attribute = type.GetCustomAttribute<EventQueueAttribute>();
            if (attribute == null)
            {
                throw new InvalidOperationException($"Queue name not defined on event {type.Name}");
            }
            return attribute.Name;
        }
    }
}
