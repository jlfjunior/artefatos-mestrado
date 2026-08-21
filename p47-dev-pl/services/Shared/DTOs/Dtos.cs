using Shared.Enums;

namespace Shared.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Lojista Lojista { get; set; }
    public string LojistaCodigo { get; set; } = null!; 
    public string LojaNome { get; set; } = null!;      
    public decimal Amount { get; set; }
    public string Type { get; set; } = null!; // "Debit" ou "Credit"
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
}

public class CreateTransactionRequest
{
    public Lojista Lojista { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = null!; // "Debit" ou "Credit"
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class ConsolidationDto
{
    public Guid Id { get; set; }
    public Lojista Lojista { get; set; }
    public string LojistaCodigo { get; set; } = null!;
    public string LojaNome { get; set; } = null!;       
    public DateTime ConsolidationDate { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
    public decimal DailyBalance { get; set; }
    public int ProcessedCount { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

public class PaginationResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class ErrorResponse
{
    public string Code { get; set; } = null!;
    public string Message { get; set; } = null!;
    public object? Details { get; set; }
}

public class HealthResponse
{
    public string Status { get; set; } = null!; 
    public Dictionary<string, string> Checks { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
