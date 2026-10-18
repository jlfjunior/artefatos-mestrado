
namespace Entries.Domain.Exceptions
{
    public abstract class DomainException(string message) : Exception(message);

    public sealed class InvalidAmountException(decimal value)
        : DomainException($"O valor '{value}' é inválido. O valor deve ser maior que zero.");

    public sealed class InvalidCurrencyException(string currency)
        : DomainException($"A moeda '{currency}' não é suportada. Use o formato ISO 4217 (por exemplo, BRL, USD).");

    public sealed class EmptyDescriptionException()
        : DomainException("A descrição da entrada não pode estar vazia.");
}