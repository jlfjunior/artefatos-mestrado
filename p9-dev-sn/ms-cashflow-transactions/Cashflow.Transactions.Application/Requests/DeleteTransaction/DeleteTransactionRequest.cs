using MediatR;

namespace Cashflow.Transactions.Application.Requests.DeleteTransaction
{
    public class DeleteTransactionRequest : IRequest
    {
        public string TransactionId { get; set; }
    }
}
