using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Enums;
using Xunit;

namespace CashFlow.Entries.Test.Domain
{
    public class EntryTests
    {
        [Fact]
        public void Entry_WithValidData_ShouldCreateValidEntry()
        {
            // Arrange
            var date = DateTime.Now;
            var value = 100.50m;
            var description = "Test entry";
            var type = EntryType.Credit;

            // Act
            var entry = new Entry(date, value, description, type);

            // Assert
            Assert.Empty(entry.Errors);
            Assert.NotEqual(Guid.Empty, entry.Id);
            Assert.Equal(date, entry.Date);
            Assert.Equal(value, entry.Value);
            Assert.Equal(description, entry.Description);
            Assert.Equal(type, entry.Type);
        }

        [Fact]
        public void Entry_WithDefaultDate_ShouldAddError()
        {
            // Arrange & Act
            var entry = new Entry(default, 100m, "Test", EntryType.Credit);

            // Assert
            Assert.Contains(entry.Errors, e => e == "Date is required.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Entry_WithInvalidValue_ShouldAddError(decimal invalidValue)
        {
            // Arrange & Act
            var entry = new Entry(DateTime.Now, invalidValue, "Test", EntryType.Credit);

            // Assert
            Assert.Contains(entry.Errors, e => e == "Value must be greater than zero.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Entry_WithInvalidDescription_ShouldAddError(string invalidDescription)
        {
            // Arrange & Act
            var entry = new Entry(DateTime.Now, 100m, invalidDescription, EntryType.Credit);

            // Assert
            Assert.Contains(entry.Errors, e => e == "Description is required.");
        }

        [Fact]
        public void Entry_WithInvalidType_ShouldAddError()
        {
            // Arrange & Act
            var entry = new Entry(DateTime.Now, 100m, "Test", (EntryType)999);

            // Assert
            Assert.Contains(entry.Errors, e => e == "Invalid entry type.");
        }

        [Fact]
        public void Entry_WithMultipleInvalidData_ShouldAddMultipleErrors()
        {
            // Arrange & Act
            var entry = new Entry(default, -1m, "", (EntryType)999);

            // Assert
            Assert.Equal(4, entry.Errors.Count());
            Assert.Contains(entry.Errors, e => e == "Date is required.");
            Assert.Contains(entry.Errors, e => e == "Value must be greater than zero.");
            Assert.Contains(entry.Errors, e => e == "Description is required.");
            Assert.Contains(entry.Errors, e => e == "Invalid entry type.");
        }
    }
}