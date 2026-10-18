namespace Lancamentos.Domain;

/// <summary>Violação de uma invariante de domínio. Sobe até a borda da API.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
