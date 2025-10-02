using EasyCash.Abstractions.Messaging.Queries;
using EasyCash.Query.Transactions.Interfaces;
using FluentValidation;

namespace EasyCash.Query.Transactions.Get;

internal sealed class GetDailyTransactionsQueryHandler : IQueryHandler<GetDailyTransactionsQuery, GetDailyTransactionsQueryResult>
{
    private readonly ITransactionQueryStore _transactionQueryStore;

    public GetDailyTransactionsQueryHandler(ITransactionQueryStore transactionQueryStore)
    {
        _transactionQueryStore = transactionQueryStore;
    }

    public async Task<GetDailyTransactionsQueryResult> Handle(GetDailyTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _transactionQueryStore.GetDailyTransactionsAsync(request, cancellationToken);
    }
}

internal sealed class GetDailyTransactionsQueryValidator : AbstractValidator<GetDailyTransactionsQuery>
{
    public GetDailyTransactionsQueryValidator()
    {
        RuleFor(x => x.Date).NotNull();
    }
}
