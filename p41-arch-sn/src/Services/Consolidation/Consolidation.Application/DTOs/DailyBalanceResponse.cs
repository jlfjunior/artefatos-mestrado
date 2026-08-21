
namespace Consolidation.Application.DTOs
{
    public sealed record DailyBalanceResponse(
        Guid Id,
        DateTime Date,
        decimal TotalCredits,
        decimal TotalDebits,
        decimal Balance,
        string Currency,
        DateTime UpdatedAt);
}