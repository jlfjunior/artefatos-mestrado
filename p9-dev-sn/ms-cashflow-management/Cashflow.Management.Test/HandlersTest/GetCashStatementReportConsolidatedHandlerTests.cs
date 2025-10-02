using Cashflow.Management.Application.Requests.GetCashStatementReportConsolidated;
using Cashflow.Management.Data.AppContext;
using Cashflow.Management.Domain.Entities;
using Cashflow.Management.Test.Helpers;

namespace Cashflow.Management.Test.HandlersTest
{
    [TestClass]
    public class GetCashStatementReportConsolidatedHandlerTests
    {
        [TestMethod]
        public async Task GetCashStatementReportConsolidated_WhenDaily()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();
            await Seed(context);

            var handler = new GetCashStatementReportConsolidatedHandler(context);

            var request = new GetCashStatementReportConsolidatedRequest
            {
                Interval = "D",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.TotalItems);
            Assert.AreEqual(1, result.TotalPages);
            Assert.AreEqual(2, result.Items.Count());
        }

        [TestMethod]
        public async Task GetCashStatementReportConsolidated_WhenMonthly()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();
            await Seed(context);

            var handler = new GetCashStatementReportConsolidatedHandler(context);

            var request = new GetCashStatementReportConsolidatedRequest
            {
                Interval = "M",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Items.Count());
            Assert.IsTrue(result.Items.All(i => i.Date.Day == 1));
        }

        [TestMethod]
        public async Task GetCashStatementReportConsolidated_WhenNoExists()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();

            var handler = new GetCashStatementReportConsolidatedHandler(context);

            var request = new GetCashStatementReportConsolidatedRequest
            {
                Interval = "D",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalItems);
            Assert.AreEqual(0, result.TotalPages);
            Assert.AreEqual(0, result.Items.Count());
        }

        private static async Task Seed(AppDbContext context)
        {
            var cashStatements = new List<CashStatement>
            {
                new() 
                {
                    OpeningDate = DateTime.Now.Date.AddDays(-1),
                    Inflow = 1000,
                    Outflow = 200,
                    Balance = 800
                },
                new() 
                {
                    OpeningDate = DateTime.Now.Date.AddMonths(-1),
                    Inflow = 2000,
                    Outflow = 500,
                    Balance = 1500
                }
            };

            context.CashStatements.AddRange(cashStatements);
            await context.SaveChangesAsync();
        }
    }
}
