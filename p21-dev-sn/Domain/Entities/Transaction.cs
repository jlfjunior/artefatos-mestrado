namespace Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Date { get; set; }  // YYYY-MM-DD
        public decimal Amount { get; set; }
        public string Type { get; set; }  // "Debit" ou "Credit"
        public string Description { get; set; }
    }
}