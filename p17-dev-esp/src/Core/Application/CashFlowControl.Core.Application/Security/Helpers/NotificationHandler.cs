using Flunt.Notifications;

namespace CashFlowControl.Core.Application.Security.Helpers
{
    public class NotificationHandler : Notifiable<Notification>, INotificationHandler
    {
        // Expor a lista de notificações como somente leitura
        public new IReadOnlyCollection<Notification> Notifications => base.Notifications;

        // Verificar se existem notificações
        public bool HasNotifications => base.Notifications.Count > 0;

        // Adicionar uma notificação individual
        public new void AddNotification(string property, string message)
        {
            base.AddNotification(property, message);
        }

        // Adicionar múltiplas notificações de uma vez
        public new void AddNotifications(IReadOnlyCollection<Notification> notifications)
        {
            base.AddNotifications(notifications);
        }
    }
}
