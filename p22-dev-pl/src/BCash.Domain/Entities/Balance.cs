namespace BCash.Domain.Entities
{
    public class Balance : BaseEntity
    {
        public decimal TotalCredit { get; set; }

        public decimal TotalDebit { get; set; }

        public DateTime Date { get; private set; }

        private Balance() { }

        public Balance(decimal totalCredit, decimal totalDebit, DateTime date)
        {
            if (totalCredit <= 0 && totalDebit <= 0) throw new ArgumentException("TotalCredit/TotalDebit must be greater than zero.");

            Id = Guid.NewGuid();
            TotalCredit = totalCredit;
            TotalDebit = totalDebit;
            Date = date;
        }
    }
}
