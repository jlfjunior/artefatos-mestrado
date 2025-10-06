namespace BCash.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public decimal Amount { get; private set; }
        
        public DateTime Date { get; private set; }
        
        public string Type { get; private set; }
        
        public string? Description { get; private set; }

        private Transaction() { }

        public Transaction(decimal amount, DateTime date, string type, string? description)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
            if (string.IsNullOrEmpty(type)) throw new ArgumentException("Type cannot be null or empty.");

            Id = Guid.NewGuid();
            Amount = amount;
            Date = date;
            Type = type;
            Description = description;
        }
    }
}
