using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CashFlow.Launches.Api;
using CashFlow.Launches.Application.Interfaces;
using CashFlow.Launches.Domain.Enums;
using CashFlow.Launches.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Launches.IntegrationTests;

public sealed class LaunchesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LaunchesApiTests(WebApplicationFactory<Program> factory)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"launches-tests-{Guid.NewGuid()}.db");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:LaunchesDatabase", $"Data Source={dbPath}");
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ILaunchIntegrationEventPublisher, NoOpLaunchIntegrationEventPublisher>();
            });
        });
    }

    [Fact]
    public async Task PostLaunches_ShouldReturnCreated_WhenPayloadIsValid()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var occurredOnUtc = DateTime.UtcNow;
        var payload = new { amount = 35.5m, type = "credit", occurredOnUtc };

        // Act
        var response = await client.PostAsJsonAsync("/api/launches", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await response.Content.ReadFromJsonAsync<CreatedLaunchResponse>();
        responseBody.Should().NotBeNull();
        responseBody!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostLaunches_ShouldReturnBadRequestWithValidationErrors_WhenPayloadIsInvalid()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var payload = new { amount = 0m, type = "credit", occurredOnUtc = DateTime.UtcNow };

        // Act
        var response = await client.PostAsJsonAsync("/api/launches", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        document.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty("Amount", out var amountErrors).Should().BeTrue();
        amountErrors.EnumerateArray().Should().Contain(e => e.GetString() == "Amount must be greater than zero.");
    }

    [Fact]
    public async Task PostLaunches_ShouldReturnBadRequestWithTypeValidation_WhenTypeIsInvalid()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var payload = new { amount = 20m, type = "invalid-type", occurredOnUtc = DateTime.UtcNow };

        // Act
        var response = await client.PostAsJsonAsync("/api/launches", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        document.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty("Type", out var typeErrors).Should().BeTrue();
        typeErrors.EnumerateArray().Should().Contain(e => e.GetString() == "Type must be either 'credit' or 'debit'.");
    }

    [Fact]
    public async Task PostLaunches_ShouldPersistLaunchToSqlite_WhenPayloadIsValid()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var occurredOnUtc = new DateTime(2026, 4, 21, 12, 30, 0, DateTimeKind.Utc);
        var payload = new { amount = 12m, type = "debit", occurredOnUtc };

        // Act
        var response = await client.PostAsJsonAsync("/api/launches", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreatedLaunchResponse>();
        created.Should().NotBeNull();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LaunchesDbContext>();
        var persistedLaunch = await dbContext.Launches.SingleAsync(x => x.Id == created!.Id);
        persistedLaunch.Amount.Should().Be(12m);
        persistedLaunch.Type.Should().Be(LaunchType.Debit);
        persistedLaunch.OccurredOnUtc.Should().Be(occurredOnUtc);
    }

    private sealed record CreatedLaunchResponse(Guid Id);

    private sealed class NoOpLaunchIntegrationEventPublisher : ILaunchIntegrationEventPublisher
    {
        public Task PublishRegisteredAsync(
            CashFlow.BuildingBlocks.IntegrationEvents.LaunchRegisteredIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
