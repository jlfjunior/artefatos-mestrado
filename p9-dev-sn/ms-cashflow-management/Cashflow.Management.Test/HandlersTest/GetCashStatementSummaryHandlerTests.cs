using Cashflow.Management.Application.Requests.GetCashStatementSummary;
using Cashflow.Management.Domain.Entities;
using Cashflow.Management.Test.Helpers;

namespace Cashflow.Management.Test.HandlersTest
{
    [TestClass]
    public class GetCashStatementSummaryHandlerTests
    {
        [TestMethod]
        public async Task GetCashStatementSummary_WhenTransactionsExist()
        {
            // Arrange
            using var dbContext = DataHelper.CreateInMemoryDbContext();

            var now = DateTime.Now;

            dbContext.CashStatements.AddRange(
                new CashStatement { OpeningDate = now.AddDays(-10), Inflow = 1000, Outflow = 200 },
                new CashStatement { OpeningDate = now.AddDays(-5), Inflow = 500, Outflow = 100 }
            );

            await dbContext.SaveChangesAsync();

            var handler = new GetCashStatementSummaryHandler(dbContext);

            // Act
            var result = await handler.Handle(new GetCashStatementSummaryRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1500, result.TotalInflows);
            Assert.AreEqual(300, result.TotalOutflows);
            Assert.AreEqual(1200, result.CurrentBalance);
        }

        [TestMethod]
        public async Task GetCashStatementSummary_WhenNoTransactions()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();

            context.CashStatements.Add(new CashStatement
            {
                OpeningDate = DateTime.Now.AddDays(-40),
                Inflow = 1000,
                Outflow = 500
            });

            await context.SaveChangesAsync();

            var handler = new GetCashStatementSummaryHandler(context);

            // Act
            var result = await handler.Handle(new GetCashStatementSummaryRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalInflows);
            Assert.AreEqual(0, result.TotalOutflows);
            Assert.AreEqual(0, result.CurrentBalance);
        }
    }
}
