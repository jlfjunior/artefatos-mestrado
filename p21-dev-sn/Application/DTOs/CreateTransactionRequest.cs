namespace Application.DTOs
{
    public class CreateTransactionRequest
    {
        public string Date { get; set; }  // Formato: YYYY-MM-DD
        public decimal Amount { get; set; }
        public string Type { get; set; }  // "Debit" ou "Credit"
        public string Description { get; set; }
    }
}