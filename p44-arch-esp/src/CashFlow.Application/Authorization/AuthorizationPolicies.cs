using CashFlow.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace CashFlow.Application.Authorization;

/// <summary>
/// Políticas de autorização baseadas em referências de acesso.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Política de autorização para leitura de usuários (funcionário).
    /// </summary>
    public const string UsersRead = "scope:users.read";
    /// <summary>
    /// Política de autorização para leitura do próprio perfil de cliente.
    /// </summary>
    public const string UsersReadSelf = "scope:users.read.self";
    /// <summary>
    /// Política de autorização para escrita de lançamentos.
    /// </summary>
    public const string TransactionsWrite = "scope:transactions.write";
    /// <summary>
    /// Política de autorização para leitura de lançamentos.
    /// </summary>
    public const string TransactionsReadAll = "scope:transactions.read.all";
    /// <summary>
    /// Política de autorização para leitura de lançamentos.
    /// </summary>
    public const string TransactionsReadSelf = "scope:transactions.read.self";
    /// <summary>
    /// Política de autorização para leitura de saldos.
    /// </summary>
    public const string BalancesReadAll = "scope:balances.read.all";
    /// <summary>
    /// Política de autorização para leitura de saldos.
    /// </summary>
    public const string BalancesReadSelf = "scope:balances.read.self";

    /// <summary>
    /// Registra as políticas de autorização.
    /// </summary>
    /// <param name="options">Opções de autorização.</param>
    public static void Register(AuthorizationOptions options)
    {
        RegisterScope(options, UsersRead, AuthorizationScopes.UsersRead);
        RegisterScope(options, UsersReadSelf, AuthorizationScopes.UsersReadSelf);
        RegisterScope(options, TransactionsWrite, AuthorizationScopes.TransactionsWrite);
        RegisterScope(options, TransactionsReadAll, AuthorizationScopes.TransactionsReadAll);
        RegisterScope(options, TransactionsReadSelf, AuthorizationScopes.TransactionsReadSelf);
        RegisterScope(options, BalancesReadAll, AuthorizationScopes.BalancesReadAll);
        RegisterScope(options, BalancesReadSelf, AuthorizationScopes.BalancesReadSelf);

        RegisterAnyScope(
            options,
            TransactionsRead,
            AuthorizationScopes.TransactionsReadAll,
            AuthorizationScopes.TransactionsReadSelf);

        RegisterAnyScope(
            options,
            BalancesRead,
            AuthorizationScopes.BalancesReadAll,
            AuthorizationScopes.BalancesReadSelf);

        RegisterAnyScope(
            options,
            ClientProfileRead,
            AuthorizationScopes.UsersRead,
            AuthorizationScopes.UsersReadSelf);
    }

    /// <summary>
    /// Política de autorização para leitura de lançamentos.
    /// </summary>
    public const string TransactionsRead = "scope:any:transactions.read";
    /// <summary>
    /// Política de autorização para leitura de saldos.
    /// </summary>
    public const string BalancesRead = "scope:any:balances.read";
    /// <summary>
    /// Política de autorização para leitura de perfil de cliente.
    /// </summary>
    public const string ClientProfileRead = "scope:any:users.read";

    private static void RegisterAnyScope(AuthorizationOptions options, string policyName, params string[] scopes) =>
        options.AddPolicy(policyName, builder => builder.AddRequirements(new AnyScopeRequirement(scopes)));

    private static void RegisterScope(AuthorizationOptions options, string policyName, string scope) =>
        options.AddPolicy(policyName, builder => builder.AddRequirements(new ScopeRequirement(scope)));
}

/// <summary>
/// Exige uma referência de acesso presente no claim scope do JWT.
/// </summary>
/// <param name="scope">Escopo a ser validado.</param>
public sealed class ScopeRequirement(string scope) : IAuthorizationRequirement
{
    /// <summary>
    /// Escopo a ser validado.
    /// </summary>
    public string Scope { get; } = scope;
}

/// <summary>
/// Valida referências de acesso declaradas no token JWT.
/// </summary>
public sealed class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
{
    /// <summary>
    /// Verifica se o token contém o escopo exigido pela política.
    /// </summary>
    /// <param name="context">Contexto da avaliação de autorização.</param>
    /// <param name="requirement">Requisito de escopo a validar.</param>
    /// <returns>Task concluída após a avaliação.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        if (ContainsScope(context, requirement.Scope))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    internal static bool ContainsScope(AuthorizationHandlerContext context, string scope)
    {
        var scopeClaim = GetScopeClaim(context.User);
        if (string.IsNullOrWhiteSpace(scopeClaim))
        {
            return false;
        }

        var scopes = scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return scopes.Contains(scope, StringComparer.Ordinal);
    }

    private static string? GetScopeClaim(System.Security.Claims.ClaimsPrincipal user) =>
        user.FindFirst("scope")?.Value ?? user.FindFirst("scp")?.Value;
}

/// <summary>
/// Exige ao menos uma referência de acesso entre as informadas.
/// </summary>
/// <param name="scopes">Escopos a ser validados.</param>
public sealed class AnyScopeRequirement(params string[] scopes) : IAuthorizationRequirement
{
    /// <summary>
    /// Escopos a ser validados.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; } = scopes;
}

/// <summary>
/// Valida referências de acesso alternativas declaradas no token JWT.
/// </summary>
public sealed class AnyScopeAuthorizationHandler : AuthorizationHandler<AnyScopeRequirement>
{
    /// <summary>
    /// Verifica se o token contém ao menos um dos escopos exigidos pela política.
    /// </summary>
    /// <param name="context">Contexto da avaliação de autorização.</param>
    /// <param name="requirement">Requisito de escopos alternativos a validar.</param>
    /// <returns>Task concluída após a avaliação.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyScopeRequirement requirement)
    {
        if (requirement.Scopes.Any(scope => ScopeAuthorizationHandler.ContainsScope(context, scope)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
