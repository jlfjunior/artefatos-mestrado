namespace EasyCash.Background.Jobs.Outbox.Models;

internal sealed record OutboxMessageResponse(Guid Id, string Content);