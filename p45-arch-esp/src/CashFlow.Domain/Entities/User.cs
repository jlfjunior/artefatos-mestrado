using CashFlow.Shared.Enums;

namespace CashFlow.Domain.Entities;

/// <summary>
/// Usuário autenticável da plataforma.
/// </summary>
public sealed class User
{
    /// <summary>Identificador único do usuário.</summary>
    public Guid Id { get; init; }

    /// <summary>Nome completo exibido na interface.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>E-mail usado no login.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Papel do usuário (funcionário ou cliente).</summary>
    public UserRole Role { get; init; }

    /// <summary>Indica se o usuário está habilitado para autenticação.</summary>
    public bool Enabled { get; init; }

    /// <summary>Instante UTC de criação do registro.</summary>
    public DateTime CreatedAt { get; init; }
}
