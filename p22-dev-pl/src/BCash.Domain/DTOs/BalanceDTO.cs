namespace BCash.Domain.DTOs
{
    public class BalanceDto
    {
        public decimal TotalCredit { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal Total
        {
            get
            {
                return TotalCredit - TotalDebit;
            }
        }

        public DateTime Date { get; set; }
    }
}
