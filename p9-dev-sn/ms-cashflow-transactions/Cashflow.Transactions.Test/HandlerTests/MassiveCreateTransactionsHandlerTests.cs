using AutoMapper;
using Cashflow.Transactions.Application.EventService;
using Cashflow.Transactions.Application.Querys;
using Cashflow.Transactions.Application.Requests.MassiveCreateTransaction;
using Cashflow.Transactions.Domain.Entities;
using Cashflow.Transactions.Domain.Enums;
using Cashflow.Transactions.Domain.Events;
using Moq;
using System.Text;

namespace Cashflow.Transactions.Test.HandlerTests
{
    [TestClass]
    public class MassiveCreateTransactionsHandlerTests
    {
        private Mock<ITransactionsQueryService> _transactionsQueryServiceMock;
        private Mock<IEventPublisherService<CreatedTransactionEvent>> _eventPublisherMock;
        private Mock<IMapper> _mapperMock;
        private MassiveCreateTransactionsHandler _handler;

        [TestInitialize]
        public void Setup()
        {
            _transactionsQueryServiceMock = new Mock<ITransactionsQueryService>();
            _eventPublisherMock = new Mock<IEventPublisherService<CreatedTransactionEvent>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new MassiveCreateTransactionsHandler(_transactionsQueryServiceMock.Object, _eventPublisherMock.Object, _mapperMock.Object);
        }

        [TestMethod]
        public async Task MassiveCreateTransactions_Success()
        {
            // Arrange: CSV content
            var csvContent = new StringBuilder();
            csvContent.AppendLine("Description,Type,Category,Amount,Date");
            csvContent.AppendLine("Recebimento,Income,Lucro,10000,2024-01-01");
            csvContent.AppendLine("Aluguel,Expense,Pagamentos,2000,2024-01-02");

            var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));

            var request = new MassiveCreateTransactionsRequest
            {
                File = fileStream
            };

            var csvData = new List<MassiveCreateTransactionsModel>
            {
                new() { Description = "Recebimento", Type = "Income", Category = "Lucro", Amount = 10000, Date = DateTime.Parse("2024-01-01") },
                new() { Description = "Aluguel", Type = "Expense", Category = "Pagamentos", Amount = 2000, Date = DateTime.Parse("2024-01-02") }
            };

            var domainData = new List<Transaction>
            {
                new() { Id = "68357a4e190b04b9907c6de8", Description = "Recebimento", Type = ETransactionType.Income, Category = "Lucro", Amount = 10000, Date = DateTime.Parse("2024-01-01") },
                new() { Id = "68357a4e190b04b9907c6de8", Description = "Aluguel", Type = ETransactionType.Expense, Category = "Pagamentos", Amount = 2000, Date = DateTime.Parse("2024-01-02") }
            };

            _mapperMock.Setup(m => m.Map<IEnumerable<Transaction>>(It.IsAny<IEnumerable<MassiveCreateTransactionsModel>>())).Returns(domainData);

            _transactionsQueryServiceMock.Setup(t => t.MassiveCreateTransactionsAsync(It.IsAny<List<Transaction>>())).Returns(Task.CompletedTask);

            _eventPublisherMock.Setup(e => e.PublishMessageAsync(It.IsAny<CreatedTransactionEvent>())).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _transactionsQueryServiceMock.Verify(t => t.MassiveCreateTransactionsAsync(It.Is<List<Transaction>>(l => l.Count() == 2)), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishMessageAsync(It.IsAny<CreatedTransactionEvent>()), Times.Exactly(2));
        }
    }
}
