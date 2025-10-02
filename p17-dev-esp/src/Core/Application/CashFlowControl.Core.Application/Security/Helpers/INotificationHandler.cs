using Flunt.Notifications;

namespace CashFlowControl.Core.Application.Security.Helpers
{
    public interface INotificationHandler
    {
        IReadOnlyCollection<Notification> Notifications { get; }
        bool HasNotifications { get; }
        void AddNotification(string property, string message);
        void AddNotifications(IReadOnlyCollection<Notification> notifications);
    }
}
