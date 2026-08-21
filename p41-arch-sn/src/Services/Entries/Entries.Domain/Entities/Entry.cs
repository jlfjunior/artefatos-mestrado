using Entries.Domain.Enums;
using Entries.Domain.Events;
using Entries.Domain.Exceptions;
using Entries.Domain.Primitives;
using Entries.Domain.ValueObjects;

namespace Entries.Domain.Entities
{
    public sealed class Entry : Entity
    {
        private Entry(
            Guid id,
            Money amount,
            EntryType type,
            string description,
            DateTime date) : base(id)
        {
            Amount = amount;
            Type = type;
            Description = description;
            Date = date;
            CreatedAt = DateTime.UtcNow;
        }

        public Money Amount { get; private set; }
        public EntryType Type { get; private set; }
        public string Description { get; private set; }
        public DateTime Date { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public static Entry Create(
            Money amount,
            EntryType type,
            string description,
            DateTime date)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new EmptyDescriptionException();

            var entry = new Entry(
                id: Guid.NewGuid(),
                amount: amount,
                type: type,
                description: description.Trim(),
                date: DateTime.SpecifyKind(date.Date, DateTimeKind.Utc));

            entry.RaiseDomainEvent(new EntryCreatedEvent(
                entryId: entry.Id,
                amount: amount.Amount,
                currency: amount.Currency,
                type: type,
                description: entry.Description,
                date: entry.Date));

            return entry;
        }

        // Construtor para reconstituição pelo EF Core
        private Entry() : base(Guid.Empty) { }
    }
}
