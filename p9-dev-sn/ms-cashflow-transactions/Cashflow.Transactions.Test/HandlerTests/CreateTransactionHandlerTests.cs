using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Requests.CreateTransaction;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Events;
using Moq;

namespace Cashflow.Transactions.Test.HandlerTests
{
    [TestClass]
    public class CreateTransactionHandlerTests
    {
        private Mock<ITransactionsQueryService> _transactionsQueryServiceMock;
        private Mock<IEventPublisherService<CreatedTransactionEvent>> _eventPublisherMock;
        private CreateTransactionHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _transactionsQueryServiceMock = new Mock<ITransactionsQueryService>();
            _eventPublisherMock = new Mock<IEventPublisherService<CreatedTransactionEvent>>();
            _handler = new CreateTransactionHandler(_transactionsQueryServiceMock.Object, _eventPublisherMock.Object);
        }

        [TestMethod]
        public async Task CreateTransaction_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateTransactionRequest
            {
                Description = "Venda de produtos",
                Type = "Income",
                Category = "Vendas",
                Amount = 1500,
                Date = DateTime.Today
            };

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _transactionsQueryServiceMock.Verify(x => x.CreateTransactionAsync(It.IsAny<Transaction>()), Times.Once);
            _eventPublisherMock.Verify(x => x.PublishMessageAsync(It.IsAny<CreatedTransactionEvent>()), Times.Once);
        }
    }
}
