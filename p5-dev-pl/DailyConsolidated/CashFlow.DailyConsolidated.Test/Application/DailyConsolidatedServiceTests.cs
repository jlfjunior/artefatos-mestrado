using CashFlow.DailyConsolidated.Application.Services;
using CashFlow.DailyConsolidated.Domain.Entities;
using CashFlow.DailyConsolidated.Domain.Interfaces;
using CashFlow.DailyConsolidated.Domain.Mocks;
using CashFlow.Entries.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CashFlow.DailyConsolidated.Test.Application
{
    public class DailyConsolidatedServiceTests
    {
        private readonly Mock<IEntryRepository> _mockEntryRepository;
        private readonly Mock<ICacheRepository> _mockCacheRepository;
        private readonly Mock<ILogger<DailyConsolidatedService>> _mockLogger;
        private readonly DailyConsolidatedService _service;

        public DailyConsolidatedServiceTests()
        {
            _mockEntryRepository = new Mock<IEntryRepository>();
            _mockCacheRepository = new Mock<ICacheRepository>();
            _mockLogger = new Mock<ILogger<DailyConsolidatedService>>();
            _service = new DailyConsolidatedService(_mockEntryRepository.Object, _mockCacheRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAsync_WhenCacheHit_ReturnsFromCache()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);
            var entries = EntriesMocks.GetEntriesMocks()
                .Where(e => DateOnly.FromDateTime(e.Date) == date)
                .ToList();
            var expectedResult = new DailyConsolidatedEntity(date, entries);

            _mockCacheRepository
                .Setup(x => x.GetDailyConsolidatedByDate(date))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetAsync(date);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockEntryRepository.Verify(x => x.GetEntriesByDate(It.IsAny<DateOnly>()), Times.Never);
            _mockCacheRepository.Verify(x => x.AddDailyConsolidation(It.IsAny<DailyConsolidatedEntity>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_WhenCacheMissAndEntriesFound_ReturnsDailyConsolidated()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);
            var entries = EntriesMocks.GetEntriesMocks()
                .Where(e => DateOnly.FromDateTime(e.Date) == date)
                .ToList();

            _mockCacheRepository
                .Setup(x => x.GetDailyConsolidatedByDate(date))
                .ReturnsAsync((DailyConsolidatedEntity)null);

            _mockEntryRepository
                .Setup(x => x.GetEntriesByDate(date))
                .ReturnsAsync(entries);

            // Act
            var result = await _service.GetAsync(date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(date, result.Date);
            Assert.Equal(entries.Count, result.Entries.Count);
            _mockCacheRepository.Verify(x => x.AddDailyConsolidation(It.IsAny<DailyConsolidatedEntity>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenCacheMissAndNoEntries_ReturnsEmptyDailyConsolidated()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);

            _mockCacheRepository
                .Setup(x => x.GetDailyConsolidatedByDate(date))
                .ReturnsAsync((DailyConsolidatedEntity)null);

            _mockEntryRepository
                .Setup(x => x.GetEntriesByDate(date))
                .ReturnsAsync((IEnumerable<Entry>)null);

            // Act
            var result = await _service.GetAsync(date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(date, result.Date);
            Assert.Empty(result.Entries);
            Assert.Equal(0m, result.DailyResult);
            _mockCacheRepository.Verify(x => x.AddDailyConsolidation(It.IsAny<DailyConsolidatedEntity>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_WhenCacheMissAndEmptyEntries_ReturnsEmptyDailyConsolidated()
        {
            // Arrange
            var date = new DateOnly(2025, 7, 27);

            _mockCacheRepository
                .Setup(x => x.GetDailyConsolidatedByDate(date))
                .ReturnsAsync((DailyConsolidatedEntity)null);

            _mockEntryRepository
                .Setup(x => x.GetEntriesByDate(date))
                .ReturnsAsync(new List<Entry>());

            // Act
            var result = await _service.GetAsync(date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(date, result.Date);
            Assert.Empty(result.Entries);
            Assert.Equal(0m, result.DailyResult);
            _mockCacheRepository.Verify(x => x.AddDailyConsolidation(It.IsAny<DailyConsolidatedEntity>()), Times.Never);
        }
    }
}