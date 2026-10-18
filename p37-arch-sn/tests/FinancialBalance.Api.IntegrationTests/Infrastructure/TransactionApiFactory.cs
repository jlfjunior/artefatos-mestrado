using System.Security.Claims;
using System.Text.Encodings.Web;
using FinancialBalance.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialBalance.Api.IntegrationTests.Infrastructure;

public class TransactionApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "FinancialBalance_Integration_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production EF Core registrations for AppDbContext.
            // Replace with TestAppDbContext (subclass) backed by InMemory.
            // Using a distinct subclass gives EF Core a unique model cache key,
            // so it builds a fresh model without the RowVersion concurrency token.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext, TestAppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace MassTransit RabbitMQ with in-memory test harness (no broker needed)
            var toRemove = services
                .Where(d => d.ServiceType?.Namespace?.StartsWith("MassTransit") == true
                         || (d.ImplementationType?.Namespace?.StartsWith("MassTransit") == true))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddMassTransitTestHarness();

            // Replace JWT with a test scheme that injects claims from X-Test-Roles header
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });

        builder.UseEnvironment("Testing");
    }

    public HttpClient CreateAuthenticatedClient(params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Roles", string.Join(",", roles));
        return client;
    }
}

/// <summary>
/// AppDbContext subclass for integration tests. Its distinct type gives EF Core
/// a separate model cache key, so OnModelCreating runs fresh and can suppress
/// the RowVersion concurrency token unsupported by the InMemory provider.
/// </summary>
public class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(DbContextOptions<TestAppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Remove RowVersion concurrency token — InMemory provider does not support it.
        var accountEntity = modelBuilder.Entity<FinancialBalance.Domain.Accounts.Account>();
        var rv = accountEntity.Metadata.FindProperty("RowVersion");
        if (rv is not null)
            accountEntity.Metadata.RemoveProperty(rv);
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuth";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Roles", out var rawRoles))
            return Task.FromResult(AuthenticateResult.NoResult());

        var roles = rawRoles.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "test@example.com"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

