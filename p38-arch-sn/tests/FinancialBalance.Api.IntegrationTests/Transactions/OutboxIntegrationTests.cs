using FinancialBalance.Api.IntegrationTests.Infrastructure;
using FinancialBalance.Domain.Accounts;
using FinancialBalance.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialBalance.Api.IntegrationTests.Transactions;

/// <summary>
/// Verifies that domain events are correctly persisted to the outbox table
/// when transactions are created or cancelled.
/// </summary>
public class OutboxIntegrationTests : IClassFixture<TransactionApiFactory>
{
    private readonly TransactionApiFactory _factory;
    private readonly HttpClient _client;

    public OutboxIntegrationTests(TransactionApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient("finance.admin");
    }

    [Fact]
    public async Task CreateTransaction_PersistsTwoOutboxMessages()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "OUTBOX-CREATE-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 1000m);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var messages = db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .ToList();

        // TransactionCreated + AccountBalanceUpdated
        messages.Should().HaveCountGreaterThanOrEqualTo(2);
        messages.Select(m => m.Type).Should().Contain(t => t.Contains("TransactionCreated"));
        messages.Select(m => m.Type).Should().Contain(t => t.Contains("AccountBalanceUpdated"));
    }

    [Fact]
    public async Task CancelTransaction_PersistsTransactionCancelledOutboxMessage()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "OUTBOX-CANCEL-001");
        var tx = await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 500m);

        // Drain existing outbox messages for this account first (mark as processed)
        using var scope1 = _factory.Services.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<AppDbContext>();
        foreach (var msg in db1.OutboxMessages.Where(m => m.ProcessedAt == null))
            msg.ProcessedAt = DateTime.UtcNow;
        await db1.SaveChangesAsync();

        await _client.PatchAsync(
            $"/api/v1/transactions/{tx.Id}/cancel?accountId={account.Id}", null);

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var newMessages = db2.OutboxMessages.Where(m => m.ProcessedAt == null).ToList();

        newMessages.Should().Contain(m => m.Type.Contains("TransactionCancelled"));
    }

    [Fact]
    public async Task OutboxMessages_HaveValidJsonPayload()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "OUTBOX-JSON-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 250m);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var messages = db.OutboxMessages.Where(m => m.ProcessedAt == null).ToList();

        foreach (var msg in messages)
        {
            msg.Payload.Should().NotBeNullOrWhiteSpace();
            var act = () => System.Text.Json.JsonDocument.Parse(msg.Payload);
            act.Should().NotThrow("outbox payload must be valid JSON");
        }
    }

    [Fact]
    public async Task OutboxMessages_TypeFieldContainsAssemblyQualifiedName()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "OUTBOX-TYPE-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 100m);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var messages = db.OutboxMessages.Where(m => m.ProcessedAt == null).ToList();

        foreach (var msg in messages)
        {
            var resolvedType = Type.GetType(msg.Type);
            resolvedType.Should().NotBeNull(
                $"Type '{msg.Type}' must resolve so OutboxProcessor can deserialize it");
        }
    }
}
