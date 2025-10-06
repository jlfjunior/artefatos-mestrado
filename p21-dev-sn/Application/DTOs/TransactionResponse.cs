namespace Application.DTOs
{
    public class TransactionResponse
    {
        public int TransactionId { get; set; }
        public string Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}