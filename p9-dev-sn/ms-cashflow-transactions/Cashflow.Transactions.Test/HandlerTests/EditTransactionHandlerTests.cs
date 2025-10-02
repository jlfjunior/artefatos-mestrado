using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Requests.EditTransaction;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Cashflow.Transactions.Domain.Events;
using Moq;

namespace Cashflow.Transactions.Test.HandlerTests
{
    [TestClass]
    public class EditTransactionHandlerTests
    {
        private Mock<ITransactionsQueryService> _transactionsQueryServiceMock;
        private Mock<IEventPublisherService<EditedTransactionEvent>> _eventPublisherMock;
        private EditTransactionHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _transactionsQueryServiceMock = new Mock<ITransactionsQueryService>();
            _eventPublisherMock = new Mock<IEventPublisherService<EditedTransactionEvent>>();
            _handler = new EditTransactionHandler(_transactionsQueryServiceMock.Object, _eventPublisherMock.Object);
        }

        [TestMethod]
        public async Task EditTransaction_EditTransactionAndPublishEvent()
        {
            // Arrange
            var transactionId = "68357a4e190b04b9907c6de8";

            var existingTransaction = new Transaction
            {
                Id = transactionId,
                Description = "Teste Transaction",
                Category = "Pagamentos",
                Type = ETransactionType.Expense,
                Amount = 100,
                Date = DateTime.UtcNow.AddDays(-1)
            };

            var request = new EditTransactionRequest
            {
                TransactionId = transactionId,
                Description = "Teste Transaction Updated",
                Category = "Pagamentos",
                Type = "Income",
                Amount = 200,
                Date = DateTime.UtcNow
            };

            _transactionsQueryServiceMock.Setup(x => x.GetTransactionByIdAsync(transactionId)).ReturnsAsync(existingTransaction);

            _transactionsQueryServiceMock.Setup(x => x.UpdateTransactionAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

            _eventPublisherMock.Setup(x => x.PublishMessageAsync(It.IsAny<EditedTransactionEvent>())).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _transactionsQueryServiceMock.Verify(x => x.UpdateTransactionAsync(It.Is<Transaction>(t =>
                t.Id == transactionId &&
                t.Description == request.Description &&
                t.Type == ETransactionType.Income &&
                t.Category == request.Category &&
                t.Amount == request.Amount &&
                t.Date == request.Date
            )), Times.Once);

            _eventPublisherMock.Verify(x => x.PublishMessageAsync(It.Is<EditedTransactionEvent>(e =>
                e.TransactionId == transactionId &&
                e.OldAmount == 100 &&
                e.NewAmount == 200 &&
                e.Category == "Pagamentos" &&
                e.Type == "Income"
            )), Times.Once);
        }
    }
}
