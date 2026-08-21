using CashFlow.Shared.Enums;

namespace CashFlow.Application.Abstractions;

/// <summary>
/// Contexto do usuário autenticado extraído do token JWT.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Verifica se o usuário está autenticado.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Obtém o ID do usuário.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Obtém o papel do usuário.
    /// </summary>
    UserRole Role { get; }

    /// <summary>
    /// Obtém os escopos do usuário.
    /// </summary>
    IReadOnlyList<string> Scopes { get; }

    /// <summary>
    /// Verifica se o usuário é um funcionário.
    /// </summary>
    bool IsEmployee { get; }

    /// <summary>
    /// Verifica se o usuário é um cliente.
    /// </summary>
    bool IsClient { get; }

    /// <summary>
    /// Verifica se o usuário tem um escopo específico.
    /// </summary>
    /// <param name="scope">Escopo a ser verificado.</param>
    /// <returns>True se o usuário tem o escopo, false caso contrário.</returns>
    bool HasScope(string scope);
}
