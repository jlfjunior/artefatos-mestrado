using Shared.Enums;

namespace Shared.Messages;

public record TransactionCreated(Guid Id, decimal Amount, TransactionType Type, DateTime CreatedAt);