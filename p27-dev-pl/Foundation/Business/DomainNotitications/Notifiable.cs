namespace Foundation.Business.DomainNotitications
{
    public abstract class Notifiable
    {
        private readonly List<string> _notifications;
        public IReadOnlyList<string> Notifications => _notifications;
        public bool IsValid => !_notifications.Any();

        protected Notifiable()
        {
            _notifications = new List<string>();
        }

        protected void AddNotification(string notification)
        {
            _notifications.Add(notification);
        }

        public abstract void Validate();
    }
}

