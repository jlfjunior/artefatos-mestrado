namespace Cashflow.Management.Domain.Entities
{
    public class CashStatement
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
        public decimal Inflow { get; set; }
        public decimal Outflow { get; set; }
        public bool isOpening { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime ClosingDate { get; set; }

        public ICollection<ConsolidatedTransactionHistory> Transactions { get; set; }

        public CashStatement OpenCash(DateTime openingDate)
        {             
            Id = Guid.NewGuid();
            isOpening = true;
            OpeningDate = openingDate;
            Balance = 0;
            Inflow = 0;
            Outflow = 0;

            return this;
        }

        public CashStatement AddTransaction(string type, decimal amount, DateTime date)
        {
            if (isOpening)
            {
                if (type == "Income")
                {
                    Inflow += amount;
                }
                else if (type == "Expense")
                {
                    Outflow += amount;
                }

                Balance = Inflow - Outflow;
            }
            else
            {
                throw new InvalidOperationException("Caixa não está aberto!");
            }

            return this;
        }

        public CashStatement CloseCash()
        {
            isOpening = false;
            ClosingDate = DateTime.Now;

            return this;
        }
    }
}
