namespace Cashflow.Management.Application.Requests.GetCashStatementSummary
{
    public class GetCashStatementSummaryResponse
    {
        public decimal CurrentBalance { get; set; }
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
    }
}
