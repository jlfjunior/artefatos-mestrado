using Flow.Core.Entities;
using Flow.Core.Repositories;
using Moq;

namespace CommomTestsUtilities.Repositories
{
    public class TransactionRepositoryBuilder
    {
        private readonly Mock<ITransactionRepository> _transactionRepository;

        public TransactionRepositoryBuilder() => _transactionRepository = new Mock<ITransactionRepository>();

        public void AddTransaction() => _transactionRepository
                .Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

        public Mock<ITransactionRepository> Builder() => _transactionRepository;
    }
}
