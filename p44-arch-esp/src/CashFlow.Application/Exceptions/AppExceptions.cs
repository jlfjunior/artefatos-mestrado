using CashFlow.Shared.Exceptions;

namespace CashFlow.Application.Exceptions;

/// <summary>
/// Exceção de regra de negócio com código e descrição expostos na API.
/// </summary>
/// <param name="error">Código estável do erro.</param>
/// <param name="description">Descrição legível do erro.</param>
public abstract class AppException(string error, string description) : CodedException(error, description);

/// <summary>
/// Acesso negado por regra de autorização de negócio (HTTP 403).
/// </summary>
/// <param name="error">Código estável do erro.</param>
/// <param name="description">Descrição legível do erro.</param>
public sealed class ForbiddenException(string error, string description) : AppException(error, description);

/// <summary>
/// Entrada ou regra de validação inválida (HTTP 400).
/// </summary>
/// <param name="error">Código estável do erro.</param>
/// <param name="description">Descrição legível do erro.</param>
public sealed class ValidationException(string error, string description) : AppException(error, description);
