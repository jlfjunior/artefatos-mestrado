using MediatR;

namespace Cashflow.Transactions.Application.Requests.GetTransactionsSummary
{
    public class GetTransactionsSummaryRequest : IRequest<IEnumerable<GetTransactionsSummaryResponse>>
    {

    }
}
