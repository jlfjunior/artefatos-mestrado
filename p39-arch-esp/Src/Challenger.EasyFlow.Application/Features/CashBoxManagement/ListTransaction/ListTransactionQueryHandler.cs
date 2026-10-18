using Challenger.EasyFlow.Application.Common.CQRS;
using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;
using FluentResults;

namespace Challenger.EasyFlow.Application.Features.CashBoxManagement.ListTransaction;

public sealed record QueryResult(int TotalCount, int Skip, int MaxPageSize, IReadOnlyCollection<TransactionViewModel> Data);

public sealed class ListTransactionQueryHandler(ITransactionRepository transactionRepository) : IQueryHandler<ListTransactionQuery, QueryResult>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<QueryResult> HandleAsync(ListTransactionQuery query, CancellationToken cancellationToken = default)
    {
        var totalTransactions = await _transactionRepository
            .CountTotalAsync(query.CashBoxId, DateOnly.FromDateTime(query.Options.Date), cancellationToken);
        var transactions = await _transactionRepository
            .ListAsync(
                query.CashBoxId,
                query.Options.Skip,
                query.Options.MaxPageSize,
                DateOnly.FromDateTime(query.Options.Date),
                cancellationToken);
        var transactionViewModels = transactions.Select(MapToViewModel);

        return new QueryResult(
            totalTransactions,
            query.Options.Skip,
            query.Options.MaxPageSize,
            [.. transactionViewModels]);
    }

    private static TransactionViewModel MapToViewModel(Transaction transaction)
        => new(
            transaction.Id,
            transaction.CashBoxId,
            transaction.CreatedAt,
            (int)transaction.Type,
            transaction.Amount,
            transaction.Description,
            transaction.Category.Id,
            transaction.PaymentMethod.Id);
}