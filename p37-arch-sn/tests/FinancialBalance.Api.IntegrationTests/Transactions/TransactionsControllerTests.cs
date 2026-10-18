using System.Net;
using System.Net.Http.Json;
using FinancialBalance.Api.IntegrationTests.Infrastructure;
using FinancialBalance.Application.Transactions.Commands.CreateTransaction;
using FinancialBalance.Domain.Accounts;
using FluentAssertions;

namespace FinancialBalance.Api.IntegrationTests.Transactions;

public class TransactionsControllerTests : IClassFixture<TransactionApiFactory>
{
    private readonly HttpClient _client;
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public TransactionsControllerTests(TransactionApiFactory factory)
        => _client = factory.CreateAuthenticatedClient("finance.admin");

    // ── CREATE TRANSACTION ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTransaction_ValidIncoming_Returns201AndUpdatesBalance()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-CREATE-001");

        var command = new CreateTransactionCommand(
            account.Id, TransactionType.Incoming, 2500m,
            "Client payment", TransactionCategory.Revenue, Today, "REF-001");
        var response = await _client.PostAsJsonAsync("/api/v1/transactions", command, Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var tx = await response.Content.ReadFromJsonAsync<Infrastructure.TransactionDto>(Helpers.JsonOptions);
        tx!.Amount.Should().Be(2500m);
        tx.Type.Should().Be(TransactionType.Incoming);
        tx.Category.Should().Be(TransactionCategory.Revenue);
        tx.Status.Should().Be(TransactionStatus.Confirmed);
        tx.ReferenceNumber.Should().Be("REF-001");
        tx.AccountId.Should().Be(account.Id);

        var balance = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var balanceDto = await balance.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);
        balanceDto!.CurrentBalance.Should().Be(2500m);
    }

    [Fact]
    public async Task CreateTransaction_ValidOutgoing_DeductsFromBalance()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-OUT-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 1000m);

        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 400m,
            TransactionCategory.Supplier);

        var balance = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await balance.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);
        dto!.CurrentBalance.Should().Be(600m);
    }

    [Fact]
    public async Task CreateTransaction_MultipleTransactions_BalanceSumsCorrectly()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-MULTI-001");

        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 3000m);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 1000m);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 500m);

        var balance = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await balance.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);
        dto!.CurrentBalance.Should().Be(3500m);
    }

    [Fact]
    public async Task CreateTransaction_AccountNotFound_Returns404()
    {
        var command = new CreateTransactionCommand(
            Guid.NewGuid(), TransactionType.Incoming, 100m,
            "Orphan", TransactionCategory.Other, Today, null);

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", command, Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTransaction_ZeroAmount_Returns400()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-ZERO-001");

        var command = new CreateTransactionCommand(
            account.Id, TransactionType.Incoming, 0m,
            "Zero", TransactionCategory.Revenue, Today, null);

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", command, Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransaction_FutureDate_Returns400()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-FUTURE-001");
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var command = new CreateTransactionCommand(
            account.Id, TransactionType.Incoming, 100m,
            "Future", TransactionCategory.Revenue, tomorrow, null);

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", command, Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransaction_EmptyDescription_Returns400()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-NODESC-001");

        var command = new CreateTransactionCommand(
            account.Id, TransactionType.Incoming, 100m, "",
            TransactionCategory.Revenue, Today, null);

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", command, Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransaction_ViewerRole_Returns403()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-AUTH-001");
        var viewerClient = new TransactionApiFactory().CreateAuthenticatedClient("finance.viewer");

        var command = new CreateTransactionCommand(
            account.Id, TransactionType.Incoming, 100m,
            "Viewer", TransactionCategory.Revenue, Today, null);

        var response = await viewerClient.PostAsJsonAsync("/api/v1/transactions", command, Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── CANCEL TRANSACTION ────────────────────────────────────────────────────

    [Fact]
    public async Task CancelTransaction_ExistingTransaction_Returns204AndReversesBalance()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-CANCEL-001");
        var tx = await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 800m);

        var cancelResponse = await _client.PatchAsync(
            $"/api/v1/transactions/{tx.Id}/cancel?accountId={account.Id}", null);

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var balance = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await balance.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);
        dto!.CurrentBalance.Should().Be(0m);
    }

    [Fact]
    public async Task CancelTransaction_AlreadyCancelledTransaction_Returns422()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-DBLCANCEL-001");
        var tx = await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 100m);

        await _client.PatchAsync($"/api/v1/transactions/{tx.Id}/cancel?accountId={account.Id}", null);

        var secondCancel = await _client.PatchAsync(
            $"/api/v1/transactions/{tx.Id}/cancel?accountId={account.Id}", null);

        secondCancel.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CancelTransaction_UnknownTransaction_Returns404()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-NOTFOUND-001");

        var response = await _client.PatchAsync(
            $"/api/v1/transactions/{Guid.NewGuid()}/cancel?accountId={account.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelTransaction_OutgoingTransaction_RestoresBalance()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-CANCEL-OUT-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 1000m);
        var outTx = await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 300m,
            TransactionCategory.Supplier);

        await _client.PatchAsync(
            $"/api/v1/transactions/{outTx.Id}/cancel?accountId={account.Id}", null);

        var balance = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await balance.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);
        dto!.CurrentBalance.Should().Be(1000m);
    }

    // ── GET TRANSACTION ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingTransaction_Returns200WithCorrectData()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-GET-001");
        var tx = await Helpers.CreateTransactionAsync(_client, account.Id,
            TransactionType.Incoming, 750m, TransactionCategory.Revenue);

        var response = await _client.GetAsync(
            $"/api/v1/transactions/{tx.Id}?accountId={account.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.TransactionDto>(Helpers.JsonOptions);
        dto!.Id.Should().Be(tx.Id);
        dto.Amount.Should().Be(750m);
        dto.Category.Should().Be(TransactionCategory.Revenue);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-GET-404-001");

        var response = await _client.GetAsync(
            $"/api/v1/transactions/{Guid.NewGuid()}?accountId={account.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── LIST TRANSACTIONS ─────────────────────────────────────────────────────

    [Fact]
    public async Task ListTransactions_ReturnsAllTransactionsForAccount()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-LIST-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 100m);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 200m);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 50m);

        var response = await _client.GetAsync($"/api/v1/transactions?accountId={account.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Infrastructure.PagedResult<Infrastructure.TransactionDto>>(Helpers.JsonOptions);
        result!.TotalCount.Should().Be(3);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task ListTransactions_FilterByType_ReturnsOnlyMatchingType()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-LIST-TYPE-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 100m);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 200m);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Outgoing, 50m);

        var response = await _client.GetAsync(
            $"/api/v1/transactions?accountId={account.Id}&type=Incoming");

        var result = await response.Content.ReadFromJsonAsync<Infrastructure.PagedResult<Infrastructure.TransactionDto>>(Helpers.JsonOptions);
        result!.TotalCount.Should().Be(2);
        result.Data.Should().OnlyContain(t => t.Type == TransactionType.Incoming);
    }

    [Fact]
    public async Task ListTransactions_FilterByStatus_ExcludesCancelled()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-LIST-STATUS-001");
        var tx1 = await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 100m);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 200m);
        await _client.PatchAsync($"/api/v1/transactions/{tx1.Id}/cancel?accountId={account.Id}", null);

        var response = await _client.GetAsync(
            $"/api/v1/transactions?accountId={account.Id}&status=Confirmed");

        var result = await response.Content.ReadFromJsonAsync<Infrastructure.PagedResult<Infrastructure.TransactionDto>>(Helpers.JsonOptions);
        result!.Data.Should().OnlyContain(t => t.Status == TransactionStatus.Confirmed);
    }

    [Fact]
    public async Task ListTransactions_FilterByDateRange_ReturnsOnlyInRange()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-LIST-DATE-001");
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 100m,
            date: pastDate);
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 200m,
            date: today);

        var response = await _client.GetAsync(
            $"/api/v1/transactions?accountId={account.Id}&from={today:yyyy-MM-dd}&to={today:yyyy-MM-dd}");

        var result = await response.Content.ReadFromJsonAsync<Infrastructure.PagedResult<Infrastructure.TransactionDto>>(Helpers.JsonOptions);
        result!.TotalCount.Should().Be(1);
        result.Data.Single().Amount.Should().Be(200m);
    }

    [Fact]
    public async Task ListTransactions_Pagination_ReturnsCorrectPage()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "TX-PAGE-001");
        for (int i = 0; i < 5; i++)
            await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming,
                (i + 1) * 100m);

        var response = await _client.GetAsync(
            $"/api/v1/transactions?accountId={account.Id}&page=1&pageSize=2");

        var result = await response.Content.ReadFromJsonAsync<Infrastructure.PagedResult<Infrastructure.TransactionDto>>(Helpers.JsonOptions);
        result!.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }
}
