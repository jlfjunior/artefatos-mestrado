using Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Messages;
using static Application.Transaction.Handlers.CreateTransaction;

namespace Application.Transaction.Handlers;

public class CreateTransaction(
    IApplicationDbContext _context,
    IPublishEndpoint publishEndpoint,
    ILogger<CreateTransaction> logger)
    : IRequestHandler<CreateTransactionCommand, Guid>
{
    public record CreateTransactionCommand(decimal Amount, TransactionType Type) : IRequest<Guid>;

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting transaction creation. Amount: {Amount}, Type: {Type}", request.Amount, request.Type);

        var transaction = TransactionEntity.Create(request.Amount, request.Type);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Transaction saved with Id: {TransactionId}", transaction.Id);

        var transactionEvent = new TransactionCreated(transaction.Id, transaction.Amount, transaction.Type, transaction.CreatedAt);

        await publishEndpoint.Publish(transactionEvent, cancellationToken);

        logger.LogInformation("TransactionCreated event published for TransactionId: {TransactionId}", transaction.Id);

        return transaction.Id;
    }
}