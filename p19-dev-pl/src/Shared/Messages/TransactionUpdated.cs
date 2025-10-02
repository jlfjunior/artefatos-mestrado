using Shared.Enums;

namespace Shared.Messages;

public record TransactionUpdated(Guid Id, decimal Amount, TransactionType Type, DateTime UpdatedAt);
