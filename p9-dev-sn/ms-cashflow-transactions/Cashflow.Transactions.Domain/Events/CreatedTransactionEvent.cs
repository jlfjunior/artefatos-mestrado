namespace Cashflow.Transactions.Domain.Events
{
    [EventQueue("created-transaction")]
    public class CreatedTransactionEvent
    {
        public string TransactionId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
