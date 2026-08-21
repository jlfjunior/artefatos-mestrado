namespace CashFlow.Shared.Messaging;

/// <summary>
/// Atributos obrigatórios da especificação CloudEvents 1.0.
/// </summary>
public static class CloudEventAttributes
{
    /// <summary>Versão da especificação CloudEvents.</summary>
    public const string SpecVersion = "1.0";

    /// <summary>Tipo do conteúdo do payload.</summary>
    public const string DataContentType = "application/json";
}

/// <summary>
/// Valores de <c>source</c> para eventos do domínio.
/// </summary>
public static class CloudEventSources
{
    /// <summary>Origem do evento de lançamentos.</summary>
    public const string TransactionsApi = "urn:cashflow:transactions-api";
}
