using CashFlow.Application.Abstractions;
using CashFlow.Shared.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CashFlow.Infrastructure.Auth;

/// <summary>
/// Resolve o usuário autenticado a partir do HttpContext.
/// </summary>
/// <param name="httpContextAccessor">Acesso ao HttpContext da requisição atual.</param>
public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    /// <summary>Verifica se o usuário está autenticado.</summary>
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <summary>Obtém o ID do usuário.</summary>
    public Guid UserId =>
        Guid.TryParse(GetClaim(ClaimTypes.NameIdentifier) ?? GetClaim("sub"), out var id)
            ? id
            : Guid.Empty;

    /// <summary>Obtém o papel do usuário.</summary>
    public UserRole Role =>
        Enum.TryParse<UserRole>(GetClaim("role"), true, out var role)
            ? role
            : default;

    /// <summary>Obtém os escopos do usuário.</summary>
    public IReadOnlyList<string> Scopes
    {
        get
        {
            var scope = GetClaim("scope");
            return string.IsNullOrWhiteSpace(scope)
                ? []
                : scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }

    /// <summary>Verifica se o usuário é um funcionário.</summary>
    public bool IsEmployee => Role == UserRole.Employee;

    /// <summary>Verifica se o usuário é um cliente.</summary>
    public bool IsClient => Role == UserRole.Client;

    /// <summary>
    /// Verifica se o usuário tem um escopo específico.
    /// </summary>
    /// <param name="scope">Escopo a ser verificado.</param>
    /// <returns>True se o usuário tem o escopo; false caso contrário.</returns>
    public bool HasScope(string scope) => Scopes.Contains(scope, StringComparer.Ordinal);

    private string? GetClaim(string type) =>
        httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value;
}
