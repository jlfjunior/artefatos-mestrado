using System.Security.Claims;
using CashFlow.Application.Authorization;
using CashFlow.Shared.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

namespace CashFlow.Application.Tests;

public class AuthorizationHandlerTests
{
    [Fact]
    public async Task ScopeAuthorizationHandler_ShouldSucceed_WhenScopeClaimMatches()
    {
        var handler = new ScopeAuthorizationHandler();
        var requirement = new ScopeRequirement(AuthorizationScopes.UsersRead);
        var context = CreateContext(requirement, CreatePrincipal("users.read other"));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task ScopeAuthorizationHandler_ShouldFail_WhenScopeClaimMissing()
    {
        var handler = new ScopeAuthorizationHandler();
        var requirement = new ScopeRequirement(AuthorizationScopes.UsersRead);
        var context = CreateContext(requirement, new ClaimsPrincipal(new ClaimsIdentity()));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task AnyScopeAuthorizationHandler_ShouldSucceed_WhenAnyScopeMatches()
    {
        var handler = new AnyScopeAuthorizationHandler();
        var requirement = new AnyScopeRequirement(
            AuthorizationScopes.TransactionsReadAll,
            AuthorizationScopes.TransactionsReadSelf);
        var context = CreateContext(requirement, CreatePrincipal("transactions.read.self"));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public void Register_ShouldAddExpectedPolicies()
    {
        var options = new AuthorizationOptions();

        AuthorizationPolicies.Register(options);

        options.GetPolicy(AuthorizationPolicies.UsersRead).Should().NotBeNull();
        options.GetPolicy(AuthorizationPolicies.TransactionsRead).Should().NotBeNull();
        options.GetPolicy(AuthorizationPolicies.BalancesRead).Should().NotBeNull();
        options.GetPolicy(AuthorizationPolicies.ClientProfileRead).Should().NotBeNull();
    }

    private static ClaimsPrincipal CreatePrincipal(string scope) =>
        new(new ClaimsIdentity([new Claim("scope", scope)]));

    private static AuthorizationHandlerContext CreateContext(
        IAuthorizationRequirement requirement,
        ClaimsPrincipal user) =>
        new([requirement], user, resource: null);
}
