using Foundation.Business.DomainNotitications;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Transactions.Business.Contracts;
using Transactions.Business.Model;

namespace Transactions.Business.UseCase
{
    public class AddTransactionUseCase : IRequestHandler<TransactionInputDto>
    {
        private readonly ILogger<AddTransactionUseCase> _logger;
        private readonly DomainNotificationContext domainNotificaionContext;
        private readonly ITransactionRepository transactionRepository;
        private readonly IBus bus;

        public AddTransactionUseCase(
            ILogger<AddTransactionUseCase> logger,
            DomainNotificationContext domainNotificaionContext, 
            ITransactionRepository transactionRepository,
            IBus bus)
        {
            _logger = logger;
            this.domainNotificaionContext = domainNotificaionContext;
            this.transactionRepository = transactionRepository;
            this.bus = bus;
        }

        public async Task Handle(TransactionInputDto request, CancellationToken cancellationToken)
        {   
            try
            {
                var (description, value, type) = request;
                var transaction = new Transaction();

                var @event = transaction.Create(description, value, type);
                transaction.Validate();

                domainNotificaionContext.AddNotifications(transaction);

                if (!domainNotificaionContext.IsValid)
                {
                    return;
                }

                await transactionRepository.Save(transaction, cancellationToken);
                await bus.Publish(@event);
            }
            catch (Exception e)
            {                
                domainNotificaionContext.AddError(e);
            }
        }
    }

}

