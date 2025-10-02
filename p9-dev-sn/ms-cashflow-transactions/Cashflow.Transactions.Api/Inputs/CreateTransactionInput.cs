namespace Cashflow.Transactions.Api.Inputs
{
    /// <summary>
    /// Model of create transaction.
    /// </summary>
    public class CreateTransactionInput
    {
        /// <summary>
        /// Short description of the transaction
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Type of transaction (Income, Expense)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Category of the transaction (Facilities, Sales, Payroll, ...)
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Amount of the transaction (12000)
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Date of transaction occurred
        /// </summary>
        public DateTime Date { get; set; }
    }
}
