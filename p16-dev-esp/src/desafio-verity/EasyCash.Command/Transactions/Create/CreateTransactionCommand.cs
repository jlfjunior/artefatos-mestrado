using EasyCash.Abstractions.Messaging.Commands;
using EasyCash.Domain.Transactions.Entities;
using EasyCash.Shared.Enums;

namespace EasyCash.Command.Transactions.Create;

public sealed class CreateTransactionCommand : IdempotencyCommandBase<CreateTransactionCommandResult>
{
    public string Description { get; set; }
    public ETransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTimeOffset? Date { get; set; }

    public TransactionEntity MapToEntity() =>
        TransactionEntity.Create(Description, Type, Amount, Category, Date);
}

public sealed class CreateTransactionCommandResult : ICommandResult
{
}