using Consolidate.Business.Contracts;
using Consolidate.Business.Model;
using Foundation.Business.DomainEvents;
using Foundation.Business.DomainNotitications;
using MassTransit;
using MediatR;

namespace Consolidate.Business.UseCase.AddConsolidateTransaction
{
    public class AddConsolidateTransaction : IRequestHandler<AddConsolidateTransactionInputDto>
    {
        private readonly DomainNotificationContext _domainNotificaionContext;
        private readonly IConsolidateTransactionRepository _consolidateTransactionRepository;
        private readonly IBus _bus;

        public AddConsolidateTransaction(
            DomainNotificationContext domainNotificaionContext,
            IConsolidateTransactionRepository consolidateTransactionRepository,
            IBus bus)
        {
            _domainNotificaionContext = domainNotificaionContext;
            _consolidateTransactionRepository = consolidateTransactionRepository;
            _bus = bus;
        }
        public async Task Handle(AddConsolidateTransactionInputDto request, CancellationToken cancellationToken)
        {
            try
            {
                IEvent @event;

                var (TransactionId, Description, Value, Date, Type) = request;
                var consolidateTransaction = await _consolidateTransactionRepository.Get(DateOnly.FromDateTime(Date), cancellationToken);

                if (consolidateTransaction == null)
                {
                    consolidateTransaction = new ConsolidateTransaction();
                    @event = consolidateTransaction.Create(TransactionId, Description, Value, Date, Type);
                }
                else
                {
                    @event = consolidateTransaction.AddTransaction(TransactionId, Description, Value, Date, Type);
                }

                consolidateTransaction.Validate();
                _domainNotificaionContext.AddNotifications(consolidateTransaction);

                if (!_domainNotificaionContext.IsValid)
                {
                    return;
                }

                await _consolidateTransactionRepository.Save(consolidateTransaction, cancellationToken);
                await _bus.Publish(@event);
            }
            catch (Exception e)
            {
                _domainNotificaionContext.AddError(e);
            }

        }
    }
}