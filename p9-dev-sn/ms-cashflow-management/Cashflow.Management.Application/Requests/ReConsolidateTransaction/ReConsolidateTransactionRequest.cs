using MediatR;

namespace Cashflow.Management.Application.Requests.ReConsolidateTransaction
{
    public class ReConsolidateTransactionRequest : IRequest<Unit>
    {
        public string TransactionId { get; set; }
        public string Type { get; set; }
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public DateTime Date { get; set; }
    }
}
