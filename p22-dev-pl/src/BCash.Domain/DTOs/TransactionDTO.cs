namespace BCash.Domain.DTOs
{
    public record TransactionDto (Guid Id, decimal Amount, DateTime Date, string? Type, string? Description);
}
