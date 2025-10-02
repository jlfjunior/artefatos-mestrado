using Cashflow.Transactions.Application.Shared;
using MediatR;

namespace Cashflow.Transactions.Application.Requests.SearchTransactions
{
    public class SearchTransactionsRequest : IRequest<PagedResponse<SearchTransactionsResponse>>
    {
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Search { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
