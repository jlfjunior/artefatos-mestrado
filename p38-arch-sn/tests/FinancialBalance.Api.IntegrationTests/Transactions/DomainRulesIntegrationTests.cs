using System.Net;
using System.Net.Http.Json;
using FinancialBalance.Api.IntegrationTests.Infrastructure;
using FinancialBalance.Application.Accounts.Commands.CreateAccount;
using FinancialBalance.Application.Transactions.Commands.CreateTransaction;
using FinancialBalance.Domain.Accounts;
using FluentAssertions;

namespace FinancialBalance.Api.IntegrationTests.Transactions;

/// <summary>
/// Tests that verify domain invariants are enforced end-to-end via the HTTP API.
/// </summary>
public class DomainRulesIntegrationTests : IClassFixture<TransactionApiFactory>
{
    private readonly HttpClient _client;
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public DomainRulesIntegrationTests(TransactionApiFactory factory)
        => _client = factory.CreateAuthenticatedClient("finance.admin");

    [Fact]
    public async Task CancelAlreadyCancelledTransaction_Returns422WithDomainMessage()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "DOM-INACTIVE-001");
        var tx = await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 100m);
        await _client.PatchAsync($"/api/v1/transactions/{tx.Id}/cancel?accountId={account.Id}", null);

        var secondCancel = await _client.PatchAsync(
            $"/api/v1/transactions/{tx.Id}/cancel?accountId={account.Id}", null);

        secondCancel.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var body = await secondCancel.Content.ReadAsStringAsync();
        body.Should().Contain("already cancelled");
    }

    [Fact]
    public async Task ErrorResponse_MatchesRfc7807ProblemDetails()
    {
        var response = await _client.GetAsync($"/api/v1/accounts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"status\":404");
        body.Should().Contain("\"title\"");
    }

    [Fact]
    public async Task ErrorResponse_ValidationFailure_ContainsErrorsExtension()
    {
        var command = new CreateTransactionCommand(
            Guid.NewGuid(), TransactionType.Incoming, -1m,
            "", TransactionCategory.Revenue, Today, null);

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", command, Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("errors");
    }

    [Fact]
    public async Task CreateAccount_ThenImmediatelyGetBalance_BalanceIsZero()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "DOM-INITIAL-BAL-001");

        var response = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);

        dto!.CurrentBalance.Should().Be(0m);
    }

    [Fact]
    public async Task CreateTransaction_BalanceReflectedImmediately()
    {
        // Verifies that SaveChangesAsync is called and data is durably committed in the same request
        var account = await Helpers.CreateAccountAsync(_client, code: "DOM-PERSIST-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 999m);

        // Fresh HTTP request — verifies data is in DB, not just in EF memory
        var response = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);

        dto!.CurrentBalance.Should().Be(999m);
    }

    [Fact]
    public async Task MultipleTransactions_AllCategories_BalanceAccumulates()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "DOM-CATS-001");

        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 1000m, TransactionCategory.Revenue);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 200m, TransactionCategory.Supplier);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 100m, TransactionCategory.Tax);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 300m, TransactionCategory.Payroll);

        var response = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);

        dto!.CurrentBalance.Should().Be(400m); // 1000 - 200 - 100 - 300
    }

    [Fact]
    public async Task CancelAndRecreateTransaction_BalanceCorrect()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "DOM-RECREATE-001");
        var tx = await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 500m);

        await _client.PatchAsync($"/api/v1/transactions/{tx.Id}/cancel?accountId={account.Id}", null);

        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 500m);

        var response = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);

        dto!.CurrentBalance.Should().Be(500m);
    }

    [Fact]
    public async Task CreateAccount_WithAllCurrencies_Succeeds()
    {
        foreach (var (currency, code) in new[]
        {
            (Currency.BRL, "CUR-BRL-001"),
            (Currency.USD, "CUR-USD-001"),
            (Currency.EUR, "CUR-EUR-001"),
        })
        {
            var response = await _client.PostAsJsonAsync("/api/v1/accounts",
                new CreateAccountCommand($"Account {currency}", code, AccountType.Checking, currency),
                Helpers.JsonOptions);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}
