namespace CashFlow.Contracts;

/// <summary>
/// Representação de um usuário cliente para consulta.
/// </summary>
/// <param name="Id">Identificador do cliente.</param>
/// <param name="FullName">Nome completo.</param>
/// <param name="Email">Endereço de e-mail de acesso.</param>
/// <param name="CreatedAt">Data de cadastro.</param>
public sealed record ClientResponse(
    Guid Id,
    string FullName,
    string Email,
    DateTime CreatedAt);
