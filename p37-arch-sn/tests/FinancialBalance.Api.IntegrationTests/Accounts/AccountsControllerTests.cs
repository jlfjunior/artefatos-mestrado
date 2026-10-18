using System.Net;
using System.Net.Http.Json;
using FinancialBalance.Api.IntegrationTests.Infrastructure;
using FinancialBalance.Application.Accounts.Commands.CreateAccount;
using FinancialBalance.Domain.Accounts;
using FluentAssertions;

namespace FinancialBalance.Api.IntegrationTests.Accounts;

public class AccountsControllerTests : IClassFixture<TransactionApiFactory>
{
    private readonly HttpClient _client;

    public AccountsControllerTests(TransactionApiFactory factory)
        => _client = factory.CreateAuthenticatedClient("finance.admin");

    // ── CREATE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAccount_ValidCommand_Returns201WithAccountDto()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountCommand("Savings Account", "SAV-001", AccountType.Savings, Currency.BRL),
            Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountDto>(Helpers.JsonOptions);
        dto.Should().NotBeNull();
        dto!.Name.Should().Be("Savings Account");
        dto.Code.Should().Be("SAV-001");
        dto.Type.Should().Be(AccountType.Savings);
        dto.Currency.Should().Be(Currency.BRL);
        dto.CurrentBalance.Should().Be(0m);
        dto.IsActive.Should().BeTrue();
        dto.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAccount_LocationHeaderPointsToGetById()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountCommand("Checking Account", "CHK-LOC-001", AccountType.Checking, Currency.USD),
            Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var getResponse = await _client.GetAsync(response.Headers.Location!.ToString());
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAccount_DuplicateCode_Returns409()
    {
        await Helpers.CreateAccountAsync(_client, code: "DUP-001");

        var response = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountCommand("Another", "DUP-001", AccountType.Checking, Currency.BRL),
            Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateAccount_EmptyName_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountCommand("", "VALID-001", AccountType.Checking, Currency.BRL),
            Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAccount_InvalidCodeChars_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountCommand("Name", "INVALID CODE!", AccountType.Checking, Currency.BRL),
            Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAccount_WithoutAdminRole_Returns403()
    {
        var operatorClient = new TransactionApiFactory().CreateAuthenticatedClient("finance.operator");
        var response = await operatorClient.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountCommand("Restricted", "RES-001", AccountType.Checking, Currency.BRL),
            Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAccount_UnauthenticatedRequest_Returns401()
    {
        var anonClient = new TransactionApiFactory().CreateClient();
        var response = await anonClient.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountCommand("Anon", "ANON-001", AccountType.Checking, Currency.BRL),
            Helpers.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET BY ID ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingAccount_Returns200WithCorrectData()
    {
        var created = await Helpers.CreateAccountAsync(_client, code: "GET-001");

        var response = await _client.GetAsync($"/api/v1/accounts/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountDto>(Helpers.JsonOptions);
        dto!.Id.Should().Be(created.Id);
        dto.Code.Should().Be("GET-001");
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/accounts/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET BALANCE ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBalance_NewAccount_ReturnsZeroBalance()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "BAL-001");

        var response = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);
        dto!.CurrentBalance.Should().Be(0m);
        dto.AccountId.Should().Be(account.Id);
    }

    [Fact]
    public async Task GetBalance_AfterIncomingTransaction_ReflectsUpdatedBalance()
    {
        var account = await Helpers.CreateAccountAsync(_client, code: "BAL-TX-001");
        await Helpers.CreateTransactionAsync(_client, account.Id, TransactionType.Incoming, 500m);

        var response = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");
        var dto = await response.Content.ReadFromJsonAsync<Infrastructure.AccountBalanceDto>(Helpers.JsonOptions);

        dto!.CurrentBalance.Should().Be(500m);
    }

    // ── LIST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAccounts_ReturnsPagedResult()
    {
        await Helpers.CreateAccountAsync(_client, code: "LST-001");
        await Helpers.CreateAccountAsync(_client, code: "LST-002");

        var response = await _client.GetAsync("/api/v1/accounts?page=1&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Infrastructure.PagedResult<Infrastructure.AccountDto>>(Helpers.JsonOptions);
        result!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListAccounts_FilterByIsActive_ReturnsOnlyMatchingAccounts()
    {
        await Helpers.CreateAccountAsync(_client, code: "ACTIVE-FILTER-001");

        var response = await _client.GetAsync("/api/v1/accounts?isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Infrastructure.PagedResult<Infrastructure.AccountDto>>(Helpers.JsonOptions);
        result!.Data.Should().OnlyContain(a => a.IsActive);
    }
}
