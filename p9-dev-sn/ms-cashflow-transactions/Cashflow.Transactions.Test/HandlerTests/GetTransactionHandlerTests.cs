using AutoMapper;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Requests.GetTransaction;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Moq;

namespace Cashflow.Transactions.Test.HandlerTests
{
    [TestClass]
    public class GetTransactionHandlerTests
    {
        private Mock<ITransactionsQueryService> _transactionsQueryServiceMock;
        private Mock<IMapper> _mapperMock;
        private GetTransactionHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _transactionsQueryServiceMock = new Mock<ITransactionsQueryService>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetTransactionHandler(_transactionsQueryServiceMock.Object, _mapperMock.Object);
        }

        [TestMethod]
        public async Task GetTransaction_ReturnTransaction()
        {
            // Arrange
            var transactionId = "68357a4e190b04b9907c6de8";
            var transaction = new Transaction
            {
                Id = transactionId,
                Description = "Test transaction",
                Category = "Venda",
                Type = ETransactionType.Income,
                Amount = 100,
                Date = DateTime.UtcNow
            };

            var expectedResponse = new GetTransactionResponse
            {
                TransactionId = transaction.Id,
                Description = transaction.Description,
                Type = transaction.Type.ToString(),
                Category = transaction.Category,
                Amount = transaction.Amount,
                Date = transaction.Date
            };

            _transactionsQueryServiceMock.Setup(x => x.GetTransactionByIdAsync(transactionId)).ReturnsAsync(transaction);

            _mapperMock.Setup(x => x.Map<GetTransactionResponse>(transaction)).Returns(expectedResponse);

            var request = new GetTransactionRequest 
            { 
                TransactionId = transactionId
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
        }
    }
}
