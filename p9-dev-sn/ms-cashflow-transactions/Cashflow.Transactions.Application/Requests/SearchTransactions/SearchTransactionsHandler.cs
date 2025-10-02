using AutoMapper;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Shared;
using MediatR;

namespace Cashflow.Transactions.Application.Requests.SearchTransactions
{
    public class SearchTransactionsHandler(ITransactionsQueryService transactionsQueryService, IMapper mapper) : IRequestHandler<SearchTransactionsRequest, PagedResponse<SearchTransactionsResponse>>
    {
        private readonly ITransactionsQueryService _transactionsQueryService = transactionsQueryService;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedResponse<SearchTransactionsResponse>> Handle(SearchTransactionsRequest request, CancellationToken cancellationToken)
        {             
            var transactions = await _transactionsQueryService.GetTransactionsAsync(request.Type, request.Category, request.Search, request.Page, request.PageSize);

            long totalItems = 0;
            long totalPages = 0;

            if (transactions.Any())
            {
                totalItems = await _transactionsQueryService.GetTransactionsTotalItemsAsync();
                totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
            }

            var items = _mapper.Map<IEnumerable<SearchTransactionsResponse>>(transactions);

            var result = new PagedResponse<SearchTransactionsResponse>()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };

            return result;
        }
    }
}
