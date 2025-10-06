using Flow.Core.Enums;

namespace Flow.Core.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public decimal Value { get; private set; }
        public TypeTransaction Type { get; private set; }
        public DateTime Date { get; private set; }

        public Transaction(decimal value, TypeTransaction type)
        {
            Id = Guid.NewGuid();
            Value = value;
            Type = type;
            Date = DateTime.Now;
        }
    }
}
