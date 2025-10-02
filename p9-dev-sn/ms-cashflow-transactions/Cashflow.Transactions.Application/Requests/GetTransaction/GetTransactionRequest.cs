using MediatR;

namespace Cashflow.Transactions.Application.Requests.GetTransaction
{
    public class GetTransactionRequest : IRequest<GetTransactionResponse>
    {
        public string TransactionId { get; set; }
    }
}
