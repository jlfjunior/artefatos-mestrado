using Entries.Domain.Enums;
using Entries.Domain.Primitives;

namespace Entries.Domain.Events
{
    public sealed class EntryCreatedEvent : IDomainEvent
    {
        public EntryCreatedEvent(
            Guid entryId,
            decimal amount,
            string currency,
            EntryType type,
            string description,
            DateTime date)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EntryId = entryId;
            Amount = amount;
            Currency = currency;
            Type = type;
            Description = description;
            Date = date;
        }

        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public Guid EntryId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public EntryType Type { get; }
        public string Description { get; }
        public DateTime Date { get; }
    }
}
