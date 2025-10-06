using Microsoft.Extensions.Logging;

namespace Foundation.Business.DomainNotitications
{
    public abstract class DomainNotificationContext
    {
        private List<string> _notifications;
        public IReadOnlyList<string> Notifications => _notifications.ToList();

        private readonly ILogger<DomainNotificationContext> _logger;

        public bool IsValid => !_notifications.Any();

        protected DomainNotificationContext(ILogger<DomainNotificationContext> logger)
        {
            _notifications = new List<string>();
            _logger = logger;
        }

        public void AddNotifications(Notifiable notifiable)
        {
            _notifications = notifiable.Notifications.ToList();
        }

        public void AddNotification(string notification) 
        { 
            _notifications.Add(notification);
        }
        
        public void AddError(Exception exception) 
        {
            AddNotification("Sorry, an error occurred :(");
            _logger.LogError(exception, "An error occurre :(");
        }
    }
}

