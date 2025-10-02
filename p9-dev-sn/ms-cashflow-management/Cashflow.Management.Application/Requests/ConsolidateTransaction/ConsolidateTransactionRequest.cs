using MediatR;

namespace Cashflow.Management.Application.Requests.ConsolidateTransaction
{
    public class ConsolidateTransactionRequest : IRequest<Unit>
    {
        public string TransactionId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
