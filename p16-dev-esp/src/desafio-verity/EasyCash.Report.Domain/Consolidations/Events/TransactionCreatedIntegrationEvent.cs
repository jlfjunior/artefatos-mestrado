using EasyCash.Abstractions;
using EasyCash.Report.Domain.Consolidations.Entity;
using EasyCash.Shared.Enums;

namespace EasyCash.Report.Domain.Consolidations.Events;

public sealed class TransactionCreatedIntegrationEvent : IntegrationEventBase
{
    public override string EndpointName => "easycash-transaction-created";

    public string Description { get; init; }
    public ETransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public string Category { get; init; }
    public DateTimeOffset Date { get; init; }

    public TransactionCreatedIntegrationEvent() { }

    public TransactionCreatedIntegrationEvent(string description
        , ETransactionType type
        , decimal amount
        , string category
        , DateTimeOffset date)
    {
        Description = description;
        Type = type;
        Amount = amount;
        Category = category;
        Date = date;
    }

    public ConsolidationTransactionEntity MapToEntity() =>
        ConsolidationTransactionEntity.Create(Description, Type, Amount, Category, Date);
}

