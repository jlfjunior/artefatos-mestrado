using CashFlow.Infrastructure.Messaging;
using CashFlow.Shared.Authorization;
using CashFlow.Shared.Enums;
using CashFlow.Shared.Messaging;
using FluentAssertions;

namespace CashFlow.Infrastructure.Tests;

public class RabbitMqTopologyTests
{
    [Fact]
    public void DeadLetterNames_ShouldDeriveFromQueueName()
    {
        const string queue = "cashflow.consolidation";

        RabbitMqTopology.DeadLetterExchangeName(queue).Should().Be("cashflow.consolidation.dlx");
        RabbitMqTopology.DeadLetterQueueName(queue).Should().Be("cashflow.consolidation.dlq");
    }
}

public class IntegrationMessageSerializerTests
{
    [Fact]
    public void SerializeAndDeserialize_ShouldRoundTripCloudEvent()
    {
        var original = TransactionCreatedMessage.Create(
            Guid.NewGuid(),
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId,
            "Venda",
            100m,
            EntryType.Credit,
            new DateOnly(2026, 6, 13));

        var bytes = IntegrationMessageSerializer.Serialize(original);
        var json = System.Text.Encoding.UTF8.GetString(bytes);

        json.Should().Contain("\"specversion\":\"1.0\"");
        json.Should().Contain("\"type\":\"cashflow.transaction.created.event\"");

        var restored = IntegrationMessageSerializer.Deserialize<TransactionCreatedMessage>(bytes);

        restored.Id.Should().Be(original.Id);
        restored.Type.Should().Be(EventExchanges.TransactionCreated);
        restored.Data.Description.Should().Be("Venda");
        restored.Data.Amount.Should().Be(100m);
        restored.Data.EntryType.Should().Be(EntryType.Credit);
    }

    [Fact]
    public void SerializeThroughInterface_ShouldIncludeDataPayload()
    {
        IIntegrationMessage message = TransactionCreatedMessage.Create(
            Guid.NewGuid(),
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId,
            "Venda",
            100m,
            EntryType.Credit,
            new DateOnly(2026, 6, 13));

        var bytes = IntegrationMessageSerializer.Serialize(message);
        var json = System.Text.Encoding.UTF8.GetString(bytes);

        json.Should().Contain("\"data\":");

        var restored = IntegrationMessageSerializer.Deserialize<TransactionCreatedMessage>(bytes);
        restored.Data.Description.Should().Be("Venda");
    }

    [Fact]
    public void ResolveExchange_ShouldUseCloudEventType()
    {
        var message = TransactionCreatedMessage.Create(
            Guid.NewGuid(),
            SeedIdentifiers.ClientUserId,
            SeedIdentifiers.EmployeeUserId,
            "Venda",
            100m,
            EntryType.Credit,
            new DateOnly(2026, 6, 13));

        IntegrationMessageSerializer.ResolveExchange(message)
            .Should().Be(EventExchanges.TransactionCreated);
    }
}

public class MessagingConstantsTests
{
    [Fact]
    public void TopologyConstants_ShouldBeStable()
    {
        EventExchanges.TransactionCreated.Should().Be("cashflow.transaction.created.event");
        MessagingConstants.ConsolidationQueue.Should().Be("cashflow.consolidation");
    }
}
