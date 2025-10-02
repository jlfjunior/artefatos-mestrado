namespace Cashflow.Transactions.Application.Requests.MassiveCreateTransaction
{
    public struct MassiveCreateTransactionsModel
    {
        public string Description { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
