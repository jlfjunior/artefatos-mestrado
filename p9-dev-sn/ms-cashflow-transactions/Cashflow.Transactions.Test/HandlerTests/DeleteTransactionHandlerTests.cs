using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Requests.DeleteTransaction;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Cashflow.Transactions.Domain.Events;
using Moq;

namespace Cashflow.Transactions.Test.HandlerTests
{
    [TestClass]
    public class DeleteTransactionHandlerTests
    {
        private Mock<ITransactionsQueryService> _transactionsQueryServiceMock;
        private Mock<IEventPublisherService<DeletedTransactionEvent>> _eventPublisherMock;
        private DeleteTransactionHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _transactionsQueryServiceMock = new Mock<ITransactionsQueryService>();
            _eventPublisherMock = new Mock<IEventPublisherService<DeletedTransactionEvent>>();
            _handler = new DeleteTransactionHandler(_transactionsQueryServiceMock.Object, _eventPublisherMock.Object);
        }

        [TestMethod]
        public async Task DeleteTransaction_DeleteAndPublishEvent()
        {
            // Arrange
            var transaction = new Transaction
            {
                Description = "Teste Transaction",
                Category = "Venda",
                Type = ETransactionType.Income,
                Amount = 150,
                Date = DateTime.UtcNow
            };

            var request = new DeleteTransactionRequest
            {
                TransactionId = transaction.Id
            };

            _transactionsQueryServiceMock.Setup(x => x.GetTransactionByIdAsync(transaction.Id)).ReturnsAsync(transaction);

            _transactionsQueryServiceMock.Setup(x => x.DeleteTransactionAsync(transaction)).Returns(Task.CompletedTask);

            _eventPublisherMock.Setup(x => x.PublishMessageAsync(It.IsAny<DeletedTransactionEvent>())).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _transactionsQueryServiceMock.Verify(x => x.GetTransactionByIdAsync(transaction.Id), Times.Once);
            _transactionsQueryServiceMock.Verify(x => x.DeleteTransactionAsync(transaction), Times.Once);
            _eventPublisherMock.Verify(x => x.PublishMessageAsync(It.IsAny<DeletedTransactionEvent>()), Times.Once);
        }
    }
}
