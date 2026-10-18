namespace CashFlow.Shared.Extensions;

/// <summary>
/// Extensões utilitárias para coleções.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Converte uma coleção em uma lista somente leitura.
    /// </summary>
    /// <typeparam name="T">Tipo dos elementos da lista.</typeparam>
    /// <param name="source">Coleção a ser convertida.</param>
    /// <returns>Lista somente leitura.</returns>
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source) =>
        source.ToList().AsReadOnly();

    /// <summary>
    /// Converte uma coleção em uma lista somente leitura.
    /// </summary>
    /// <typeparam name="T">Tipo dos elementos da coleção de origem.</typeparam>
    /// <typeparam name="TResult">Tipo dos elementos da lista resultante.</typeparam>
    /// <param name="source">Coleção a ser convertida.</param>
    /// <param name="selector">Função de seleção dos elementos.</param>
    /// <returns>Lista somente leitura.</returns>
    public static IReadOnlyList<TResult> ToReadOnlyList<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) =>
        source.Select(selector).ToList().AsReadOnly();
}
