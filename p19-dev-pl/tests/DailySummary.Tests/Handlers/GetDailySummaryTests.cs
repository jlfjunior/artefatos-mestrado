using Application.DailySummary.Handlers;
using Application.DTOs;
using Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using static Application.DailySummary.Handlers.GetDailySummary;

namespace DailySummary.Tests.Handlers;

[TestFixture]
public class GetDailySummaryHandlerTests : BaseTests
{
    private GetDailySummary _handler = null!;
    private Mock<IDistributedCache> _cacheMock = null!;

    [SetUp]
    public void SetUp()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _handler = new GetDailySummary(DbContext, Mapper, _cacheMock.Object);
    }

    [Test]
    public async Task Handle_ExistingSummary_ShouldReturnDailySummaryDto()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var summary = DailySummaryEntity.Create(date, 500.00m, 200.00m);
        DbContext.DailySummaries.Add(summary);
        await DbContext.SaveChangesAsync();

        var cacheKey = $"daily-summary:{date:yyyy-MM-dd}";
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var query = new GetDailySummaryQuery(date);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Date, Is.EqualTo(date));
            Assert.That(result.TotalCredits, Is.EqualTo(summary.TotalCredits));
            Assert.That(result.TotalDebits, Is.EqualTo(summary.TotalDebits));
            Assert.That(result.Balance, Is.EqualTo(summary.TotalCredits - summary.TotalDebits));
        });

        _cacheMock.Verify(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_NonExistingSummary_ShouldReturnNull()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var query = new GetDailySummaryQuery(DateTime.UtcNow.Date);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null);

        _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_CachedSummary_ShouldReturnCachedData()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var summaryDto = new DailySummaryDTO
        {
            Date = date,
            TotalCredits = 1000.00m,
            TotalDebits = 400.00m,
            Balance = 600.00m
        };

        var query = new GetDailySummaryQuery(date);
        var cachedData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(summaryDto);

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cachedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Date, Is.EqualTo(summaryDto.Date));
            Assert.That(result.TotalCredits, Is.EqualTo(summaryDto.TotalCredits));
            Assert.That(result.TotalDebits, Is.EqualTo(summaryDto.TotalDebits));
            Assert.That(result.Balance, Is.EqualTo(summaryDto.Balance));
        });

        _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}