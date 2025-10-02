using AutoMapper;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Shared;
using MediatR;

namespace Cashflow.Transactions.Application.Requests.GetTransaction
{
    public class GetTransactionHandler(ITransactionsQueryService transactionsQueryService, IMapper _mapper) : IRequestHandler<GetTransactionRequest, GetTransactionResponse>
    {
        private readonly ITransactionsQueryService _transactionsQueryService = transactionsQueryService;
        private readonly IMapper _mapper = _mapper;

        public async Task<GetTransactionResponse> Handle(GetTransactionRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.TransactionId))
                throw new BusinessException("Id da transação inválido!");

            var transaction = await _transactionsQueryService.GetTransactionByIdAsync(request.TransactionId);

            if (transaction == null)
                throw new NotFoundException("Transação não encontrada!");

            var result = _mapper.Map<GetTransactionResponse>(transaction);

            return result;
        }
    }
}
