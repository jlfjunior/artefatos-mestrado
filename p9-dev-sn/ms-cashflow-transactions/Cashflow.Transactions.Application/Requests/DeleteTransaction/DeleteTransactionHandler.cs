using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Shared;
using Cashflow.Transactions.Domain.Events;
using MediatR;

namespace Cashflow.Transactions.Application.Requests.DeleteTransaction
{
    public class DeleteTransactionHandler(ITransactionsQueryService transactionsQueryService, IEventPublisherService<DeletedTransactionEvent> eventPublisher) : IRequestHandler<DeleteTransactionRequest>
    {
        private readonly ITransactionsQueryService _transactionsQueryService = transactionsQueryService;
        private readonly IEventPublisherService<DeletedTransactionEvent> _eventPublisher = eventPublisher;

        public async Task Handle(DeleteTransactionRequest request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionsQueryService.GetTransactionByIdAsync(request.TransactionId);

            if (transaction == null)
                throw new NotFoundException("Transação não encontrada!");

            try
            {
                await _transactionsQueryService.DeleteTransactionAsync(transaction);

                var @event = new DeletedTransactionEvent
                {
                    TransactionId = transaction.Id,
                    Type = transaction.Type.ToString(),
                    Amount = transaction.Amount
                };

                await _eventPublisher.PublishMessageAsync(@event);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
