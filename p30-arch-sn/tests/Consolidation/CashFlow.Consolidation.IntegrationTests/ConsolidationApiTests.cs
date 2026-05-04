using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CashFlow.Consolidation.Api;
using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Consolidation.IntegrationTests;

public sealed class ConsolidationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConsolidationApiTests(WebApplicationFactory<Program> factory)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"consolidation-api-tests-{Guid.NewGuid()}.db");
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:ConsolidationDatabase", $"Data Source={dbPath}");
        });
    }

    [Fact]
    public async Task GetByDate_ShouldReturnOkWithBody_WhenDateExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
        var consolidation = DailyConsolidation.Create(new DateOnly(2026, 4, 21), 100m, 40m).Value;
        await dbContext.DailyConsolidations.AddAsync(consolidation);
        await dbContext.SaveChangesAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/consolidations/2026-04-21");
        var responseBody = await response.Content.ReadFromJsonAsync<DailyConsolidationPayload>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().NotBeNull();
        responseBody!.Date.Should().Be(new DateOnly(2026, 4, 21));
        responseBody.TotalCredits.Should().Be(100m);
        responseBody.TotalDebits.Should().Be(40m);
        responseBody.Balance.Should().Be(60m);
    }

    [Fact]
    public async Task GetByDate_ShouldReturnNotFound_WhenDateDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/consolidations/2026-04-25");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByPeriod_ShouldReturnOrderedList_WhenRangeIsValid()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
        await dbContext.DailyConsolidations.AddRangeAsync(
            DailyConsolidation.Create(new DateOnly(2026, 4, 22), 120m, 20m).Value,
            DailyConsolidation.Create(new DateOnly(2026, 4, 21), 100m, 10m).Value);
        await dbContext.SaveChangesAsync();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/consolidations?startDate=2026-04-21&endDate=2026-04-22");
        var responseBody = await response.Content.ReadFromJsonAsync<List<DailyConsolidationPayload>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().NotBeNull();
        responseBody!.Select(x => x.Date).Should().ContainInOrder(new DateOnly(2026, 4, 21), new DateOnly(2026, 4, 22));
    }

    [Fact]
    public async Task GetByPeriod_ShouldReturnBadRequestWithValidationProblem_WhenDatesAreInvalid()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/consolidations?startDate=not-a-date&endDate=2026-04-22");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        document.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty("StartDate", out var startErrors).Should().BeTrue();
        startErrors.EnumerateArray().Should().ContainSingle()
            .Which.GetString().Should().Be("StartDate must be a valid date (yyyy-MM-dd).");
    }

    private sealed record DailyConsolidationPayload(
        DateOnly Date,
        decimal TotalCredits,
        decimal TotalDebits,
        decimal Balance);
}
