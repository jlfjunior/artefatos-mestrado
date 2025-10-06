namespace Domain.Entities
{
    public class DailyBalance
    {
        public string Date { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal Balance { get; set; }
    }
}