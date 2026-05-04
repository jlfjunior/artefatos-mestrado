using CashFlow.Lancamentos.Api.Infrastructure;
using CashFlow.Shared;

namespace CashFlow.Lancamentos.Api.Application;

public sealed class ListEntriesHandler
{
    private readonly ILedgerQueryService _ledgerQueryService;

    public ListEntriesHandler(ILedgerQueryService ledgerQueryService)
    {
        _ledgerQueryService = ledgerQueryService;
    }

    public Task<IReadOnlyList<RegisterEntryResponse>> HandleAsync(
        string merchantId,
        DateOnly businessDate,
        CancellationToken cancellationToken)
        => _ledgerQueryService.ListByBusinessDateAsync(merchantId, businessDate, cancellationToken);
}

