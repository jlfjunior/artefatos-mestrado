using AutoMapper;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Requests.GetTransactionsSummary;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Moq;

namespace Cashflow.Transactions.Test.HandlerTests
{
    [TestClass]
    public class GetTransactionsSummaryHandlerTests
    {
        private Mock<ITransactionsQueryService> _transactionsQueryServiceMock;
        private Mock<IMapper> _mapperMock;
        private GetTransactionsSummaryHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _transactionsQueryServiceMock = new Mock<ITransactionsQueryService>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetTransactionsSummaryHandler(_transactionsQueryServiceMock.Object, _mapperMock.Object);
        }

        [TestMethod]
        public async Task GetTransactions_ReturnSummary()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new() { Description = "Test Transaction 1", Type = ETransactionType.Income, Category = "Venda", Amount = 150, Date = DateTime.Now },
                new() { Description = "Test Transaction 2", Type = ETransactionType.Income, Category = "Lucro", Amount = 50, Date = DateTime.Now }
            };

            var expectedResponse = new List<GetTransactionsSummaryResponse>
            {
                new() { TransactionId = "68357a4e190b04b9907c6de8", Description = "Test Transaction 1", Type = ETransactionType.Income.ToString(), Amount = 150, Date = DateTime.Now },
                new() { TransactionId = "68357a4e190b04b9907c6de7", Description = "Test Transaction 2", Type = ETransactionType.Income.ToString(), Amount = 50, Date = DateTime.Now }
            };

            _transactionsQueryServiceMock.Setup(x => x.GetTransactionsSummaryAsync()).ReturnsAsync(transactions);

            _mapperMock.Setup(x => x.Map<IEnumerable<GetTransactionsSummaryResponse>>(transactions)).Returns(expectedResponse);

            var request = new GetTransactionsSummaryRequest();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(expectedResponse.ToList(), result.ToList());
        }
    }
}
