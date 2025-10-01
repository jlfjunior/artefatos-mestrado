using Xunit;
using CashFlow.DailyConsolidated.Domain.Entities;
using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Enums;

namespace CashFlow.DailyConsolidated.Test.Domain
{
    public class DailyConsolidatedEntityTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);
            var entries = new List<Entry>
            {
                new Entry(new DateTime(2025, 7, 27), 100.00m, "Credit entry", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 27), 50.00m, "Debt entry", EntryType.Debt)
            };

            // Act
            var dailyConsolidated = new DailyConsolidatedEntity(date, entries);

            // Assert
            Assert.Equal(date, dailyConsolidated.Date);
            Assert.Equal(entries, dailyConsolidated.Entries);
            Assert.Equal(50.00m, dailyConsolidated.DailyResult);
        }

        [Fact]
        public void CalculateResult_WithEmptyEntries_ShouldReturnZero()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);
            var entries = new List<Entry>();

            // Act
            var dailyConsolidated = new DailyConsolidatedEntity(date, entries);

            // Assert
            Assert.Equal(0m, dailyConsolidated.DailyResult);
        }

        [Fact]
        public void CalculateResult_WithOnlyCreditEntries_ShouldReturnPositiveSum()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);
            var entries = new List<Entry>
            {
                new Entry(new DateTime(2025, 7, 27), 100.00m, "Credit 1", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 27), 200.00m, "Credit 2", EntryType.Credit)
            };

            // Act
            var dailyConsolidated = new DailyConsolidatedEntity(date, entries);

            // Assert
            Assert.Equal(300.00m, dailyConsolidated.DailyResult);
        }

        [Fact]
        public void CalculateResult_WithOnlyDebtEntries_ShouldReturnNegativeSum()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);
            var entries = new List<Entry>
            {
                new Entry(new DateTime(2025, 7, 27), 100.00m, "Debt 1", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 27), 200.00m, "Debt 2", EntryType.Debt)
            };

            // Act
            var dailyConsolidated = new DailyConsolidatedEntity(date, entries);

            // Assert
            Assert.Equal(-300.00m, dailyConsolidated.DailyResult);
        }

        [Fact]
        public void CalculateResult_WithMixedEntries_ShouldReturnCorrectBalance()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);
            var entries = new List<Entry>
            {
                new Entry(new DateTime(2025, 7, 27), 500.00m, "Credit 1", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 27), 200.00m, "Debt 1", EntryType.Debt),
                new Entry(new DateTime(2025, 7, 27), 300.00m, "Credit 2", EntryType.Credit),
                new Entry(new DateTime(2025, 7, 27), 100.00m, "Debt 2", EntryType.Debt)
            };

            // Act
            var dailyConsolidated = new DailyConsolidatedEntity(date, entries);

            // Assert
            Assert.Equal(500.00m, dailyConsolidated.DailyResult);
        }
    }
}