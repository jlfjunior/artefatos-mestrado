using EasyCash.Abstractions.Interfaces;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Report.Domain.Consolidations.Events;
using EasyCash.Report.Domain.Consolidations.Interfaces;
using MediatR;

namespace EasyCash.Report.Command.Consolidations.Transactions;

internal sealed class ConsolidationTransactionCreatedIntegrationEventHandler : INotificationHandler<TransactionCreatedIntegrationEvent>
{
    private readonly IConsolidationTransactionCommandStore _consolidationTransactionCommandStore;
    private readonly IUnitOfWork _unitOfWork;

    public ConsolidationTransactionCreatedIntegrationEventHandler(IConsolidationTransactionCommandStore consolidationTransactionCommandStore
        , IUnitOfWork unitOfWork)
    {
        _consolidationTransactionCommandStore = consolidationTransactionCommandStore;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var entity = notification.MapToEntity();

        await Save(cancellationToken, entity);
    }

    private async Task Save(CancellationToken cancellationToken, ConsolidationTransactionEntity entity)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        await _consolidationTransactionCommandStore.AddAsync(entity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        transaction.Commit();
    }
}