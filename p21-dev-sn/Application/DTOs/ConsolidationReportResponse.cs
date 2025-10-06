namespace Application.DTOs
{
    public class ConsolidationReportResponse
    {
        public string Date { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal DailyBalance { get; set; }
    }
}