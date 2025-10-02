using Flunt.Notifications;

namespace CashFlowControl.Core.Application.Exceptions
{
    public class CommandValidationException : Exception
    {
        public IReadOnlyCollection<Notification> Notifications { get; }

        public CommandValidationException(IReadOnlyCollection<Notification> notifications)
        {
            Notifications = notifications;
        }

        public override string ToString()
        {
            return string.Join(", ", Notifications.Select(n => $"{n.Key}: {n.Message}"));
        }
    }
}
