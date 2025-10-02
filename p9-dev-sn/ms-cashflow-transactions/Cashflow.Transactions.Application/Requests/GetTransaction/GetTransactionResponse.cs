namespace Cashflow.Transactions.Application.Requests.GetTransaction
{
    public class GetTransactionResponse
    {
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
