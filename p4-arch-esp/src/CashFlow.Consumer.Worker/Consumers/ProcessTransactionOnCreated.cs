using CashFlow.Application.Transactions;
using CashFlow.Domain.Transactions;
using MassTransit;

namespace CashFlow.Consumer.Worker.Consumers
{
    public class ProcessTransactionOnCreated : IConsumer<TransactionCreated>
    {
        private readonly ILogger<ProcessTransactionOnCreated> _logger;
        private readonly ITransactionLockProcessor _transactionLockProcessor;
        public ProcessTransactionOnCreated(ILogger<ProcessTransactionOnCreated> logger,
            ITransactionLockProcessor transactionLockProcessor)
        {
            _logger = logger;
            _transactionLockProcessor = transactionLockProcessor;
        }

        public async Task Consume(ConsumeContext<TransactionCreated> context)
        {
            _logger.LogInformation("Processing transaction created event for transaction id: {TransactionId}", context.Message.Id);

            var accountId = context.Message.AccountId;
            var transactionId = context.Message.Id;

            await _transactionLockProcessor.DoAsync(accountId: accountId, transactionId: transactionId, context.CancellationToken);

            _logger.LogInformation("Finished processing transaction created event for transaction id: {TransactionId}", context.Message.Id);
        }
    }

    public static class ReceiveEndpointConfiguration
    {
        public static void AddProcessTransactionCreatedConsumer(this IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
        {
            configurator.ReceiveEndpoint(nameof(ProcessTransactionOnCreated), ep =>
            {
                ep.ConfigureConsumer<ProcessTransactionOnCreated>(context);

                ep.Bind(nameof(TransactionCreated), config =>
                {
                    config.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout;
                    config.Durable = true;
                });
            });
        }
    }
}
