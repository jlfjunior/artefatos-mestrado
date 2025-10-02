using AutoMapper;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Requests.SearchTransactions;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Moq;

namespace Cashflow.Transactions.Test.HandlerTests
{
    [TestClass]
    public class SearchTransactionsHandlerTests
    {
        private Mock<ITransactionsQueryService> _transactionsQueryServiceMock;
        private Mock<IMapper> _mapperMock;
        private SearchTransactionsHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _transactionsQueryServiceMock = new Mock<ITransactionsQueryService>();
            _mapperMock = new Mock<IMapper>();
            _handler = new SearchTransactionsHandler(_transactionsQueryServiceMock.Object, _mapperMock.Object);
        }

        [TestMethod]
        public async Task SearchTransactions_ReturnPagedResponse()
        {
            // Arrange
            var request = new SearchTransactionsRequest
            {
                Type = "",
                Category = "",
                Search = "",
                Page = 1,
                PageSize = 10
            };

            var transactions = new List<Transaction>
            {
                new() { Id = "68357a4e190b04b9907c6de8", Description = "Teste", Type = ETransactionType.Income, Category = "Venda", Amount = 5000, Date = DateTime.Now },
                new() { Id = "68357a4e190b04b9907c6de7", Description = "Teste 2", Type = ETransactionType.Expense, Category = "Aluguel", Amount = 1500, Date = DateTime.Now },
            };

            var responseList = new List<SearchTransactionsResponse>
            {
                new() { TransactionId = "68357a4e190b04b9907c6de8", Description = "Teste", Type = ETransactionType.Income.ToString(), Category = "Venda", Amount = 5000, Date = DateTime.Now },
                new() { TransactionId = "68357a4e190b04b9907c6de7", Description = "Teste 2", Type = ETransactionType.Expense.ToString(), Category = "Aluguel", Amount = 1500, Date = DateTime.Now }
            };

            _transactionsQueryServiceMock.Setup(x => x.GetTransactionsAsync(request.Type, request.Category, request.Search, request.Page, request.PageSize)).ReturnsAsync(transactions);

            _transactionsQueryServiceMock.Setup(x => x.GetTransactionsTotalItemsAsync()).ReturnsAsync(1);

            _mapperMock.Setup(x => x.Map<IEnumerable<SearchTransactionsResponse>>(transactions)).Returns(responseList);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalItems);
            Assert.AreEqual(1, result.TotalPages);
            Assert.AreEqual(responseList.Count, result.Items.Count());
            Assert.AreEqual(1, result.Page);
            Assert.AreEqual(10, result.PageSize);
        }

    }
}
