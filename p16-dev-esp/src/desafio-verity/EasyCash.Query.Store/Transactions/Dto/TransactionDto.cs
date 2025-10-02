using EasyCash.Query.Transactions.Get;
using EasyCash.Shared.Enums;

namespace EasyCash.Query.Store.Transactions.Dto;

internal sealed class TransactionDto
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTimeOffset Date { get; set; }

    public static GetDailyTransactionsQueryResultItem MapToResult(TransactionDto dto) =>
        new()
        {
            Id = dto.Id,
            Description = dto.Description,
            Type = Enum.Parse<ETransactionType>(dto.Type),
            Amount = dto.Amount,
            Category = dto.Category,
            Date = dto.Date
        };
}

