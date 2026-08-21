namespace CashFlow.Shared.Enums;

/// <summary>
/// Tipo do lançamento financeiro: crédito aumenta o saldo, débito reduz.
/// </summary>
public enum EntryType
{
    /// <summary>Crédito aumenta o saldo.</summary>
    Credit = 1,
    /// <summary>Débito reduz o saldo.</summary>
    Debit = 2
}
