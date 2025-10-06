using Flow.Application.MessageBrokers;
using Flow.Core.DTOs;
using Flow.Core.Repositories;
using Flow.Exceptions;

namespace Flow.Application.UseCases.Transaction
{
    public class ExecuteTransactionUseCase : IExecuteTransactionUseCase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnityOfWork _unityOfWork;
        private readonly IKafkaProducer _kafkaProducer;
        public ExecuteTransactionUseCase(
            ITransactionRepository transactionRepository,
            IUnityOfWork unityOfWork,
            IKafkaProducer kafkaProducer)
        {
            _transactionRepository = transactionRepository;
            _unityOfWork = unityOfWork;
            _kafkaProducer = kafkaProducer;
        }
        
        public async Task Execute(TransactionDTO request)
        {
            var transaction = new Flow.Core.Entities.Transaction(request.Value, request.Type);
            var transactions = await _transactionRepository.Transactions();
            await _transactionRepository.AddTransaction(transaction);
            await _unityOfWork.Commit();
            await _kafkaProducer.ProduceAsync(transaction);
        }

        private static void Validate(TransactionDTO request)
        {
            var validator = new ExecuteTransactionValidator();

            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                throw new FlowException(result.ToString());
            }
        }
    }
}