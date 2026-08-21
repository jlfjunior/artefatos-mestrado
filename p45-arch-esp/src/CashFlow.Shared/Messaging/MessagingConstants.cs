namespace CashFlow.Shared.Messaging;

/// <summary>
/// Exchanges de Event Message (EIP) — uma exchange por evento de domínio.
/// </summary>
public static class EventExchanges
{
    /// <summary>Exchange para o evento de lançamento criado.</summary>
    public const string TransactionCreated = "cashflow.transaction.created.event";

    /// <summary>Todas as exchanges de evento publicadas/consumidas pela solução.</summary>
    public static IReadOnlyList<string> All { get; } = [TransactionCreated];
}

/// <summary>
/// Nomes de filas do broker (detalhe de infraestrutura).
/// </summary>
public static class MessagingConstants
{
    /// <summary>Nome da fila para o consumidor de consolidação.</summary>
    public const string ConsolidationQueue = "cashflow.consolidation";
}
