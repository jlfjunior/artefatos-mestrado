namespace Cashflow.Transactions.Domain.Events
{
    [EventQueue("deleted-transaction")]
    public class DeletedTransactionEvent
    {
        public string TransactionId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
    }
}
