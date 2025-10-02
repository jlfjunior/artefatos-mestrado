using Cashflow.Management.Application.Requests.ConsolidateTransaction;
using Cashflow.Management.Application.Shared;
using Cashflow.Management.Domain.Entities;
using Cashflow.Management.Test.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Test.HandlersTest
{
    [TestClass]
    public class ConsolidateTransactionHandlerTests
    {
        [TestMethod]
        public async Task ConsolidateTransaction_WhenTransactionIsNew()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();

            var handler = new ConsolidateTransactionHandler(context);

            var request = new ConsolidateTransactionRequest
            {
                TransactionId = "68357a4e190b04b9907c6de8",
                Date = DateTime.Now,
                Type = "Income",
                Amount = 1000
            };

            // Act
            await handler.Handle(request, CancellationToken.None);

            // Assert
            var cashStatement = await context.CashStatements.FirstOrDefaultAsync();
            var history = await context.ConsolidatedTransactionHistories.FirstOrDefaultAsync();

            Assert.IsNotNull(cashStatement);
            Assert.AreEqual(1000, cashStatement.Inflow);
            Assert.IsNotNull(history);
            Assert.AreEqual(request.TransactionId, history.TransactionId);
        }

        [TestMethod]
        public async Task ConsolidateTransaction_WhenTransactionAlreadyConsolidated()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();

            var existingId = "68357a4e190b04b9907c6de8";

            await context.ConsolidatedTransactionHistories.AddAsync(new ConsolidatedTransactionHistory
            {
                TransactionId = existingId,
                Date = DateTime.Now
            });

            await context.SaveChangesAsync();

            var handler = new ConsolidateTransactionHandler(context);
            var request = new ConsolidateTransactionRequest
            {
                TransactionId = existingId,
                Date = DateTime.Now,
                Type = "Expense",
                Amount = 500
            };

            // Assert
            await Assert.ThrowsExceptionAsync<BusinessException>(async () => await handler.Handle(request, CancellationToken.None));
        }

        [TestMethod]
        public async Task ConsolidateTransaction_WhenExists()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();

            var today = DateTime.Now.Date;

            var existingStatement = new CashStatement();
            existingStatement.OpenCash(today);

            await context.CashStatements.AddAsync(existingStatement);
            await context.SaveChangesAsync();

            var handler = new ConsolidateTransactionHandler(context);
            var request = new ConsolidateTransactionRequest
            {
                TransactionId = "68357a4e190b04b9907c6de8",
                Date = today,
                Type = "Income",
                Amount = 700
            };

            // Act
            await handler.Handle(request, CancellationToken.None);

            // Assert
            var updatedStatement = await context.CashStatements.FirstOrDefaultAsync();
            Assert.IsNotNull(updatedStatement);
            Assert.AreEqual(700, updatedStatement.Inflow);
        }
    }
}
