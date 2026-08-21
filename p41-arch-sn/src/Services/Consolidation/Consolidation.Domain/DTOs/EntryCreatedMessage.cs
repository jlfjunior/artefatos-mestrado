
namespace Consolidation.Domain.DTOs
{
    public sealed record EntryCreatedMessage(
        Guid EventId,
        Guid EntryId,
        decimal Amount,
        string Currency,
        int Type,
        string Description,
        DateTime Date,
        DateTime OccurredOn);
}
