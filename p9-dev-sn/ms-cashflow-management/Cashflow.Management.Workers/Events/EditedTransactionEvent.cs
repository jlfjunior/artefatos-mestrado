namespace Cashflow.Management.Workers.Events
{
    [EventQueue("edited-transaction")]
    public class EditedTransactionEvent
    {
        public string TransactionId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public DateTime Date { get; set; }
    }
}
