
namespace Consolidation.Domain.Entities
{
    public sealed class DailyBalance
    {
        private DailyBalance(
            Guid id,
            DateTime date,
            decimal totalCredits,
            decimal totalDebits,
            string currency)
        {
            Id = id;
            Date = date;
            TotalCredits = totalCredits;
            TotalDebits = totalDebits;
            Currency = currency;
            UpdatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public DateTime Date { get; private set; }
        public decimal TotalCredits { get; private set; }
        public decimal TotalDebits { get; private set; }
        public string Currency { get; private set; }
        public decimal Balance => TotalCredits - TotalDebits;
        public DateTime UpdatedAt { get; private set; }

        public static DailyBalance Create(DateTime date, string currency) =>
            new(Guid.NewGuid(), date.Date, 0, 0, currency);

        public void ApplyCredit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("O valor do crédito deve ser maior que zero.", nameof(amount));

            TotalCredits += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ApplyDebit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("O valor do débito deve ser maior que zero.", nameof(amount));

            TotalDebits += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        // Construtor para reconstituição pelo EF Core
        private DailyBalance() { }
    }
}
