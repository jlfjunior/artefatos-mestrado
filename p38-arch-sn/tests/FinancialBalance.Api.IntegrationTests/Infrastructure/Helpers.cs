using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialBalance.Application.Accounts.Commands.CreateAccount;
using FinancialBalance.Application.Transactions.Commands.CreateTransaction;
using FinancialBalance.Domain.Accounts;

namespace FinancialBalance.Api.IntegrationTests.Infrastructure;

public static class Helpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<AccountDto> CreateAccountAsync(
        HttpClient client,
        string name = "Test Account",
        string code = "TEST-001",
        AccountType type = AccountType.Checking,
        Currency currency = Currency.BRL)
    {
        var command = new CreateAccountCommand(name, code, type, currency);
        var response = await client.PostAsJsonAsync("/api/v1/accounts", command, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountDto>(JsonOptions))!;
    }

    public static async Task<TransactionDto> CreateTransactionAsync(
        HttpClient client,
        Guid accountId,
        TransactionType type = TransactionType.Incoming,
        decimal amount = 1000m,
        TransactionCategory category = TransactionCategory.Revenue,
        DateOnly? date = null)
    {
        var command = new CreateTransactionCommand(
            accountId, type, amount, "Integration test transaction",
            category, date ?? DateOnly.FromDateTime(DateTime.UtcNow), null);

        var response = await client.PostAsJsonAsync("/api/v1/transactions", command, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TransactionDto>(JsonOptions))!;
    }
}

// Lightweight response DTOs used only by the integration test project for deserialisation.
// These mirror the shapes returned by the API but avoid re-exporting Application layer types.

public record AccountDto(
    Guid Id, string Name, string Code,
    AccountType Type, Currency Currency,
    decimal CurrentBalance, bool IsActive, DateTime CreatedAt);

public record TransactionDto(
    Guid Id, Guid AccountId,
    TransactionType Type, decimal Amount, string Description,
    TransactionCategory Category, string? ReferenceNumber,
    TransactionStatus Status, DateOnly TransactionDate, DateTime CreatedAt);

public record AccountBalanceDto(Guid AccountId, decimal CurrentBalance, string Currency, DateTime AsOf);

public record PagedResult<T>(IReadOnlyList<T> Data, int TotalCount, int Page, int PageSize);
