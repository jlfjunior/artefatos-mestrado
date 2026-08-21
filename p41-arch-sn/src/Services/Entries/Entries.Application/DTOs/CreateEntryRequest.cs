using Entries.Domain.Enums;

namespace Entries.Application.DTOs
{
    public sealed record CreateEntryRequest(
        decimal Amount,
        string Currency,
        EntryType Type,
        string Description,
        DateTime Date);
}