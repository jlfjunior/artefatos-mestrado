using MediatR;

namespace Cashflow.Transactions.Application.Requests.MassiveCreateTransaction
{
    public class MassiveCreateTransactionsRequest : IRequest
    {
        public Stream File { get; set; }
    }
}
