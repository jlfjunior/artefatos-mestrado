using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Shared;
using Cashflow.Transactions.Domain.Enums;
using Cashflow.Transactions.Domain.Events;
using MediatR;

namespace Cashflow.Transactions.Application.Requests.EditTransaction
{
    public class EditTransactionHandler(ITransactionsQueryService transactionsQueryService, IEventPublisherService<EditedTransactionEvent> eventPublisher) : IRequestHandler<EditTransactionRequest>
    {
        private readonly ITransactionsQueryService _transactionsQueryService = transactionsQueryService;
        private readonly IEventPublisherService<EditedTransactionEvent> _eventPublisher = eventPublisher;

        public async Task Handle(EditTransactionRequest request, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(ETransactionType), request.Type))
                throw new BusinessException("Tipo de transação inválido!");

            if (request.Category.Length < 3)
                throw new BusinessException("Categoria inválido!");

            if (request.Amount < 0)
                throw new BusinessException("Valor da transação inválido!");

            var transaction = await _transactionsQueryService.GetTransactionByIdAsync(request.TransactionId);

            if (transaction == null)
                throw new NotFoundException("Transação não encontrada!");

            var oldAmount = transaction.Amount;

            transaction.Description = request.Description;
            transaction.Type = (ETransactionType)Enum.Parse(typeof(ETransactionType), request.Type);
            transaction.Category = request.Category;
            transaction.Amount = request.Amount;
            transaction.Date = request.Date;
            transaction.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _transactionsQueryService.UpdateTransactionAsync(transaction);

                var @event = new EditedTransactionEvent
                {
                    TransactionId = transaction.Id,
                    Category = transaction.Category,
                    Type = transaction.Type.ToString(),
                    OldAmount = oldAmount,
                    NewAmount = transaction.Amount,
                    Date = transaction.Date
                };

                await _eventPublisher.PublishMessageAsync(@event);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return;
        }
    }
}
