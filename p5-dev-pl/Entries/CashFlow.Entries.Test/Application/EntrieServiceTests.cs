using CashFlow.Entries.Application.Service;
using CashFlow.Entries.Domain.Entities;
using CashFlow.Entries.Domain.Enums;
using CashFlow.Entries.Domain.Exceptions;
using CashFlow.Entries.Domain.Interfaces;
using Moq;
using Xunit;

namespace CashFlow.Entries.Test.Application
{
    public class EntrieServiceTests
    {
        private readonly Mock<IEntryRepository> _repositoryMock;
        private readonly EntrieService _service;

        public EntrieServiceTests()
        {
            _repositoryMock = new Mock<IEntryRepository>();
            _service = new EntrieService(_repositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldReturnCreatedEntry()
        {
            // Arrange
            var value = 100.50m;
            var description = "Test entry";
            var type = EntryType.Credit;
            var date = DateTime.Now;

            var expectedEntry = new Entry(date, value, description, type);
            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Entry>()))
                          .ReturnsAsync(expectedEntry);

            // Act
            var result = await _service.CreateAsync(value, description, type, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(value, result.Value);
            Assert.Equal(description, result.Description);
            Assert.Equal(type, result.Type);
            Assert.Equal(date, result.Date);
            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Entry>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithoutDate_ShouldUseCurrentDate()
        {
            // Arrange
            var value = 100.50m;
            var description = "Test entry";
            var type = EntryType.Credit;

            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Entry>()))
                          .ReturnsAsync((Entry e) => e);

            // Act
            var result = await _service.CreateAsync(value, description, type, null);

            // Assert
            Assert.NotNull(result);
            Assert.True((DateTime.Now - result.Date).TotalSeconds < 1);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidData_ShouldThrowEntityValidationFailException()
        {
            // Arrange
            var value = -100m; // Invalid value
            var description = "Test entry";
            var type = EntryType.Credit;
            var date = DateTime.Now;

            // Act & Assert
            await Assert.ThrowsAsync<EntityValidationFailException>(() => 
                _service.CreateAsync(value, description, type, date));
            
            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Entry>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenRepositoryThrowException_ShouldThrowException()
        {
            // Arrange
            var value = 100.50m;
            var description = "Test entry";
            var type = EntryType.Credit;
            var date = DateTime.Now;

            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Entry>()))
                          .Throws(new Exception());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateAsync(value, description, type, date));
            
            Assert.Equal("Failed to create entry.", exception.Message);
        }

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new EntrieService(null!));
            Assert.Equal("entrieRepository", exception.ParamName);
        }
    }
}