namespace Cashflow.Transactions.Data.Configs
{
    public class CashflowDbConfig
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; } = null!;
        public string TransactionsCollection { get; set; } = null!;
    }
}
