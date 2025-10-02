using EasyCash.Abstractions.Interfaces;
using EasyCash.Domain.Transactions.Events;
using EasyCash.Domain.Transactions.Interfaces;
using MediatR;

namespace EasyCash.Command.Transactions.Create.Events;

internal sealed class TransactionCreatedIntegrationEventHandler : INotificationHandler<TransactionCreatedIntegrationEvent>
{
    private readonly ITransactionsCommandStore _transactionsCommandStore;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionCreatedIntegrationEventHandler(ITransactionsCommandStore transactionsCommandStore
        , IUnitOfWork unitOfWork)
    {
        _transactionsCommandStore = transactionsCommandStore;
        _unitOfWork = unitOfWork;
    }


    public async Task Handle(TransactionCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var entity = notification.MapToEntity();

        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        await _transactionsCommandStore.AddAsync(entity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        transaction.Commit();
    }
}

