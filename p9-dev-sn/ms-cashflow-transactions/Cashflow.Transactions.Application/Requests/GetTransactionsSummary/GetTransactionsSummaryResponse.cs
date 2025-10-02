namespace Cashflow.Transactions.Application.Requests.GetTransactionsSummary
{
    public class GetTransactionsSummaryResponse
    {
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }
}
