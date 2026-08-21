using FinancialBalance.Application.Common;
using FinancialBalance.Application.Transactions.Commands.CreateTransaction;
using FinancialBalance.Domain.Accounts;
using MediatR;

namespace FinancialBalance.Application.Transactions.Queries.ListTransactions;

public class ListTransactionsQueryHandler : IRequestHandler<ListTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly IAccountRepository _repository;

    public ListTransactionsQueryHandler(IAccountRepository repository)
        => _repository = repository;

    public async Task<PagedResult<TransactionDto>> Handle(ListTransactionsQuery request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new NotFoundException($"Account {request.AccountId} not found.");

        var (items, total) = await _repository.ListTransactionsAsync(
            account.Id,
            request.Type,
            request.Category,
            request.Status,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(CreateTransactionCommandHandler.ToDto).ToList();
        return new PagedResult<TransactionDto>(dtos, request.Page, request.PageSize, total);
    }
}
