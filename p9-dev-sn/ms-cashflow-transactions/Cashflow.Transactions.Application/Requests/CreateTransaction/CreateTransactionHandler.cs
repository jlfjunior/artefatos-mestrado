using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Shared;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Cashflow.Transactions.Domain.Events;
using MediatR;

namespace Cashflow.Transactions.Application.Requests.CreateTransaction
{
    public class CreateTransactionHandler(ITransactionsQueryService transactionsQueryService, IEventPublisherService<CreatedTransactionEvent> eventPublisher) : IRequestHandler<CreateTransactionRequest>
    {
        private readonly ITransactionsQueryService _transactionsQueryService = transactionsQueryService;
        private readonly IEventPublisherService<CreatedTransactionEvent> _eventPublisher = eventPublisher;

        public async Task Handle(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(ETransactionType), request.Type))
                throw new BusinessException("Tipo de transação inválido!");

            if(request.Category.Length < 3)
                throw new BusinessException("Categoria inválido!");

            if(request.Amount < 0)
                throw new BusinessException("Valor da transação inválido!");

            var transaction = new Transaction
            {
                Description = request.Description,
                Type = (ETransactionType)Enum.Parse(typeof(ETransactionType), request.Type),
                Category = request.Category,
                Amount = request.Amount,
                Date = request.Date,
                CreatedAt = DateTime.UtcNow,
            };

            try
            {
                await _transactionsQueryService.CreateTransactionAsync(transaction);

                var @event = new CreatedTransactionEvent
                {
                    TransactionId = transaction.Id,
                    Category = transaction.Category,
                    Type = transaction.Type.ToString(),
                    Amount = transaction.Amount,
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
