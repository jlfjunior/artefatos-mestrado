using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Domain.Entities;
using MassTransit;

namespace CashFlowControl.Core.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBus _messageBus;


        public TransactionService(ITransactionRepository transactionRepository, IBus messageBus)
        {
            _transactionRepository = transactionRepository;
            _messageBus = messageBus;
        }

        public async Task<TransactionCreatedDTO> CreateTransactionAsync(CreateTransactionDTO createTransaction)
        {
            var transactionCreated = new TransactionCreatedDTO
            {
                Amount = createTransaction.Amount,
                Type = createTransaction.Type
            };

            try
            {
                await _messageBus.Publish(transactionCreated);

                return transactionCreated;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while sending the transaction to the message queue.", ex);
            }
        }

        public async Task<List<Transaction>?> GetAllTransactionsAsync()
        {
            return await _transactionRepository.GetAllTransactionsAsync();
        }

        public async Task<List<Transaction>?> GetTransactionByDateAsync(DateTime date)
        {
            return await _transactionRepository.GetTransactionByDateAsync(date);
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
        {
            return await _transactionRepository.GetTransactionByIdAsync(id);
        }
    }
}
