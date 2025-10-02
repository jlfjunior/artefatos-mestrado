using AutoMapper;
using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CashFlowControl.DailyConsolidation.Infrastructure.Messaging.Consumers
{
    public class TransactionCreatedConsumer : IConsumer<TransactionCreatedDTO>
    {
        private readonly ILogger<TransactionCreatedConsumer> _logger;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDailyConsolidationService _dailyConsolidationService;
        private readonly IMapper _mapper;
        private readonly IBus _messageBus;

        public TransactionCreatedConsumer(ILogger<TransactionCreatedConsumer> logger, ITransactionRepository transactionRepository, IDailyConsolidationService dailyConsolidationService, IMapper mapper, IBus messageBus)
        {
            _logger = logger;
            _transactionRepository = transactionRepository;
            _dailyConsolidationService = dailyConsolidationService;
            _mapper = mapper;
            _messageBus = messageBus;
        }

        public async Task Consume(ConsumeContext<TransactionCreatedDTO> context)
        {
            var message = context.Message;

            _logger.LogInformation("Received transaction: Amount {Amount}, Type {Type}, CreatedAt {CreatedAt}",
                message.Amount, message.Type, message.CreatedAt);

            var transaction = new Transaction
            {
                Amount = message.Amount,
                Type = message.Type,
                CreatedAt = message.CreatedAt
            };

            try
            {
                await _transactionRepository.CreateTransactionAsync(transaction);

                var createTransactionDTO = _mapper.Map<CreateTransactionDTO>(transaction);

                await _dailyConsolidationService.ProcessTransactionAsync(createTransactionDTO);

            }
            catch (Exception)
            {
                var transactionCreatedFailed = _mapper.Map<TransactionCreatedFailedDTO>(transaction);

                await _messageBus.Publish(transactionCreatedFailed);
            }

            await Task.CompletedTask;
        }
    }
}
