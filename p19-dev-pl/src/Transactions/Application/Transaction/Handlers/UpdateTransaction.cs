using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Messages;
using static Application.Transaction.Handlers.UpdateTransaction;

namespace Application.Transaction.Handlers;

public class UpdateTransaction(IApplicationDbContext _context, IPublishEndpoint publishEndpoint)
    : IRequestHandler<UpdateTransactionCommand, bool>
{
    public record UpdateTransactionCommand(Guid Id, decimal Amount, TransactionType Type) : IRequest<bool>;

    public async Task<bool> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction is null)
            return false;

        transaction.Update(request.Amount, request.Type);
        await _context.SaveChangesAsync(cancellationToken);

        var transactionEvent = new TransactionUpdated(request.Id, request.Amount, request.Type, DateTime.UtcNow);
        await publishEndpoint.Publish(transactionEvent, cancellationToken);

        return true;
    }
}