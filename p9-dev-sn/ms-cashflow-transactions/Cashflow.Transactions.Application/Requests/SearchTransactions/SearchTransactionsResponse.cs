namespace Cashflow.Transactions.Application.Requests.SearchTransactions
{
    public class SearchTransactionsResponse
    {
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
