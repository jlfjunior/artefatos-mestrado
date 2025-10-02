using AutoMapper;
using Cashflow.Transactions.Application.Querys;
using MediatR;
using MongoDB.Driver;

namespace Cashflow.Transactions.Application.Requests.GetTransactionsSummary
{
    public class GetTransactionsSummaryHandler(ITransactionsQueryService transactionsQueryService, IMapper mapper) : IRequestHandler<GetTransactionsSummaryRequest, IEnumerable<GetTransactionsSummaryResponse>>
    {
        private readonly ITransactionsQueryService _transactionsQueryService = transactionsQueryService;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<GetTransactionsSummaryResponse>> Handle(GetTransactionsSummaryRequest request, CancellationToken cancellationToken)
        {
            var transactions = await _transactionsQueryService.GetTransactionsSummaryAsync();

            if (!transactions.Any())
                return [];

            var result = _mapper.Map<IEnumerable<GetTransactionsSummaryResponse>>(transactions);

            return result;
        }
    }
}
