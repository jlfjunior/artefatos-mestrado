namespace Cashflow.Management.Domain.Entities
{
    public class ConsolidatedTransactionHistory
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public DateTime Date { get; set; }

        public Guid CashStatementId { get; set; }
        public CashStatement CashStatement { get; set; }

        public ConsolidatedTransactionHistory AddConsolidatedTransactionHistory(string transactionId, DateTime date, Guid cashStatementId)
        {
            Id = Guid.NewGuid();
            TransactionId = transactionId;
            Date = date;
            CashStatementId = cashStatementId;

            return this;
        }
    }
}
