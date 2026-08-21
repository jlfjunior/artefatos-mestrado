using Entries.Domain.Enums;

namespace Entries.Application.DTOs
{
    public sealed record EntryResponse(
        Guid Id,
        decimal Amount,
        string Currency,
        EntryType Type,
        string Description,
        DateTime Date,
        DateTime CreatedAt);
}