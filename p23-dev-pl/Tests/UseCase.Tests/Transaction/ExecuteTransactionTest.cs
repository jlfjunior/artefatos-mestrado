using CommomTestsUtilities.Repositories;
using Flow.Application.MessageBrokers;
using Flow.Application.UseCases.Transaction;
using Flow.Core.DTOs;
using Flow.Core.Entities;
using Flow.Core.Repositories;
using Moq;

namespace UseCase.Tests.NovaPasta
{
    public class ExecuteTransactionTest
    {
        [Fact]
        public async Task Success()
        {
            var executeTransactionUseCase = CreateUseCase(out var transactionRepository, out var unityOfWork, out var kafkaProducer);

            var transactionDTO = new TransactionDTO
            {
                Value = 100,
                Type = Flow.Core.Enums.TypeTransaction.Credit
            };

            await executeTransactionUseCase.Execute(transactionDTO);

            transactionRepository.Verify(
                repo => repo.AddTransaction(
                    It.Is<Transaction>(t => t.Value == transactionDTO.Value && t.Type == transactionDTO.Type)
                ), Times.Once);

            unityOfWork.Verify(uow => uow.Commit(), Times.Once);

            kafkaProducer.Verify(
                prod => prod.ProduceAsync(
                    It.Is<Transaction>(t => t.Value == transactionDTO.Value && t.Type == transactionDTO.Type)
                ), Times.Once);
        }

        private static ExecuteTransactionUseCase CreateUseCase(
            out Mock<ITransactionRepository> transactionRepository,
            out Mock<IUnityOfWork> unityOfWork,
            out Mock<IKafkaProducer> kafkaProducer)
        {
            transactionRepository = new TransactionRepositoryBuilder().Builder();
            unityOfWork = new UnityOfWorkBuilder().Builder();
            kafkaProducer = new KafkaProducerBuilder().Builder();

            return new ExecuteTransactionUseCase(transactionRepository.Object, unityOfWork.Object, kafkaProducer.Object);
        }
    }
}
