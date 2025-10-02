using MediatR;

namespace Cashflow.Transactions.Application.Requests.CreateTransaction
{
    public class CreateTransactionRequest : IRequest
    {
        public string Description { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
