using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using static Application.Transaction.Handlers.DeleteTransaction;

namespace Application.Transaction.Handlers;

public class DeleteTransaction(IApplicationDbContext _context, IPublishEndpoint publishEndpoint)
    : IRequestHandler<DeleteTransactionCommand, bool>
{
    public record DeleteTransactionCommand(Guid Id) : IRequest<bool>;

    public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction is null)
            return false;

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        var transactionEvent = new TransactionDeleted(request.Id);
        await publishEndpoint.Publish(transactionEvent, cancellationToken);

        return true;
    }
}