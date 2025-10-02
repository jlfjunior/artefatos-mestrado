namespace Cashflow.Management.Domain.Entities
{
    public class ConsolidatedCashBalance
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
    }
}
