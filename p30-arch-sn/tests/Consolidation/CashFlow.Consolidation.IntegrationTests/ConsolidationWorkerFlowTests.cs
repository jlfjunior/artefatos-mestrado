using CashFlow.BuildingBlocks.Behaviors;
using CashFlow.BuildingBlocks.IntegrationEvents;
using CashFlow.Consolidation.Application.CommandHandlers;
using CashFlow.Consolidation.Infrastructure;
using CashFlow.Consolidation.Infrastructure.Persistence;
using CashFlow.Consolidation.Worker;
using CashFlow.Consolidation.Worker.Consumers;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CashFlow.Consolidation.IntegrationTests;

public sealed class ConsolidationWorkerFlowTests
{
    [Fact]
    public async Task ConsumeAsync_ShouldConsolidateOnceAndRemainIdempotent_WhenDuplicateLaunchRegisteredEvent()
    {
        // Arrange
        var dbPath = Path.Combine(Path.GetTempPath(), $"consolidation-worker-tests-{Guid.NewGuid()}.db");
        using var host = BuildHost(dbPath);

        await host.StartAsync();
        using (var migrationScope = host.Services.CreateScope())
        {
            var migrationDbContext = migrationScope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
            await migrationDbContext.Database.MigrateAsync();
        }

        var launchId = Guid.NewGuid();
        var registeredEvent = new LaunchRegisteredIntegrationEvent(
            launchId,
            30m,
            "credit",
            new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc));

        // Act
        using (var consumeScope = host.Services.CreateScope())
        {
            var consumer = consumeScope.ServiceProvider.GetRequiredService<LaunchRegisteredConsumer>();
            await consumer.ConsumeAsync(registeredEvent, CancellationToken.None);
            await consumer.ConsumeAsync(registeredEvent, CancellationToken.None);
        }

        // Assert
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
        var daily = dbContext.DailyConsolidations.Single(x => x.Date == new DateOnly(2026, 4, 21));
        var processedEvents = dbContext.ProcessedLaunchEvents.ToList();

        daily.TotalCredits.Should().Be(30m);
        daily.Balance.Should().Be(30m);
        processedEvents.Should().ContainSingle(x => x.LaunchId == launchId);

        await host.StopAsync();
    }

    private static IHost BuildHost(string dbPath)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ConsolidationDatabase"] = $"Data Source={dbPath}",
                ["RabbitMq:HostName"] = "localhost"
            })
            .Build();

        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(ProcessLaunchRegisteredEventCommandHandler).Assembly);
                });
                services.AddValidatorsFromAssemblyContaining<ProcessLaunchRegisteredEventCommandHandler>();
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                services.AddInfrastructure(configuration);
                services.AddScoped<LaunchRegisteredConsumer>();
            })
            .Build();
    }
}
