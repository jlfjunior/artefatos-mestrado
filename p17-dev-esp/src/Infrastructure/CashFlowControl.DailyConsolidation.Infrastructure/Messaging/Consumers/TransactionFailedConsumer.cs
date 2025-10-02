using AutoMapper;
using CashFlowControl.Core.Application.DTOs;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CashFlowControl.DailyConsolidation.Infrastructure.Messaging.Consumers
{
    public class TransactionFailedConsumer : IConsumer<TransactionCreatedFailedDTO>
    {
        private readonly ILogger<TransactionFailedConsumer> _logger;
        private readonly IBus _messageBus;
        private readonly IMapper _mapper;
        public TransactionFailedConsumer(ILogger<TransactionFailedConsumer> logger, IBus messageBus, IMapper mapper)
        {
            _logger = logger;
            _messageBus = messageBus;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<TransactionCreatedFailedDTO> context)
        {
            var message = context.Message;

            _logger.LogError("Transaction processing failed and moved to DLQ: Amount {Amount}, Type {Type}, CreatedAt {CreatedAt}",
                message.Amount, message.Type, message.CreatedAt);

            var transactionCreated = _mapper.Map<TransactionCreatedDTO>(message);

            await _messageBus.Publish(transactionCreated);

            await Task.CompletedTask;
        }
    }
}
