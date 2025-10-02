namespace Cashflow.Management.Application.Requests.GetCashStatementReportConsolidated
{
    public class GetCashStatementReportConsolidatedResponse
    {
        public DateTime Date { get; set; }
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal TotalBalance { get; set; }
    }
}
