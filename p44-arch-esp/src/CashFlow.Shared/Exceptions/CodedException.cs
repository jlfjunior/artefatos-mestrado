namespace CashFlow.Shared.Exceptions;

/// <summary>
/// Exceção com código estável e descrição legível para respostas de erro da API.
/// </summary>
/// <param name="error">Código estável do erro.</param>
/// <param name="description">Descrição legível do erro.</param>
public class CodedException(string error, string description) : Exception(description)
{
    /// <summary>Código estável do erro.</summary>
    public string Error { get; } = error;
}
