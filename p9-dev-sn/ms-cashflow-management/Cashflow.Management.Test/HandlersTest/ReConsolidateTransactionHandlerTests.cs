using Cashflow.Management.Application.Requests.ReConsolidateTransaction;
using Cashflow.Management.Data.AppContext;
using Cashflow.Management.Domain.Entities;
using Cashflow.Management.Test.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Test.HandlersTest
{
    [TestClass]
    public class ReConsolidateTransactionHandlerTests
    {
        [TestMethod]
        public async Task ReConsolidateTransaction_WhenAmountChanges()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();
            await Seed(context);

            var handler = new ReConsolidateTransactionHandler(context);

            var request = new ReConsolidateTransactionRequest
            {
                TransactionId = "68357a4e190b04b9907c6de8",
                OldAmount = 500,
                NewAmount = 700,
                Type = "Income",
                Date = DateTime.Today
            };

            // Act
            await handler.Handle(request, CancellationToken.None);

            // Assert
            var statement = await context.CashStatements.FirstAsync();
            Assert.AreEqual(1000 - 500 + 700, statement.Inflow);
            Assert.AreEqual(statement.Inflow - statement.Outflow, statement.Balance);
        }

        [TestMethod]
        public async Task ReConsolidateTransaction_WhenDateChanges()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();
            await Seed(context);

            var handler = new ReConsolidateTransactionHandler(context);

            var newDate = DateTime.Today.AddDays(-1);

            var request = new ReConsolidateTransactionRequest
            {
                TransactionId = "68357a4e190b04b9907c6de8",
                OldAmount = 300,
                NewAmount = 300,
                Type = "Expense",
                Date = newDate
            };

            // Act
            await handler.Handle(request, CancellationToken.None);

            var oldStatement = await context.CashStatements.FirstAsync(c => c.OpeningDate == DateTime.Today);
            var newStatement = await context.CashStatements.FirstAsync(c => c.OpeningDate == newDate);

            // Assert
            Assert.AreEqual(200 - 300, oldStatement.Outflow);
            Assert.AreEqual(300, newStatement.Outflow);
            Assert.AreEqual(oldStatement.Inflow - oldStatement.Outflow, oldStatement.Balance);
            Assert.AreEqual(newStatement.Inflow - newStatement.Outflow, newStatement.Balance);
        }

        [TestMethod]
        public async Task ReConsolidateTransaction_DoesNotAlter()
        {
            // Arrange
            using var context = DataHelper.CreateInMemoryDbContext();
            await Seed(context);

            var handler = new ReConsolidateTransactionHandler(context);

            var request = new ReConsolidateTransactionRequest
            {
                TransactionId = "68357a4e190b04b9907c6de8",
                OldAmount = 100,
                NewAmount = 100,
                Type = "Income",
                Date = DateTime.Today
            };

            var before = await context.CashStatements.AsNoTracking().FirstAsync();
            
            // Act
            await handler.Handle(request, CancellationToken.None);

            var after = await context.CashStatements.FirstAsync();

            // Assert
            Assert.AreEqual(before.Inflow, after.Inflow);
            Assert.AreEqual(before.Outflow, after.Outflow);
            Assert.AreEqual(before.Balance, after.Balance);
        }

        private static async Task Seed(AppDbContext context)
        {
            var statement = new CashStatement
            {
                Id = Guid.NewGuid(),
                OpeningDate = DateTime.Today,
                Inflow = 1000,
                Outflow = 200,
                Balance = 800
            };

            var transaction = new ConsolidatedTransactionHistory
            {
                Id = Guid.NewGuid(),
                TransactionId = "68357a4e190b04b9907c6de8",
                CashStatement = statement,
                CashStatementId = statement.Id
            };

            await context.CashStatements.AddAsync(statement);
            await context.ConsolidatedTransactionHistories.AddAsync(transaction);
            await context.SaveChangesAsync();
        }
    }
}
