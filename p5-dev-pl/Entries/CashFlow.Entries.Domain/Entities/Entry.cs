using CashFlow.Entries.Domain.Enums;

namespace CashFlow.Entries.Domain.Entities
{
    public class Entry : Entity
    {
        public Guid Id { get; private set; }
        public DateTime Date { get; private set; }
        public decimal Value { get; private set; }
        public string Description { get; private set; }
        public EntryType Type { get; private set; }

        public Entry(DateTime date, decimal value, string description, EntryType type)
        {
            Id = Guid.NewGuid();
            Date = date;
            Value = value;
            Description = description;
            Type = type;

            Validate();
        }

        public override void Validate()
        {
            if (Date == default)
            {
                AddError("Date is required.");
            }

            if (Value <= 0)
            {
                AddError("Value must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                AddError("Description is required.");
            }

            if (!Enum.IsDefined(typeof(EntryType), Type))
            {
                AddError("Invalid entry type.");
            }
        }
    }
}
